using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Piceon.Services
{
    public static class BackendConctroller
    {
        public static bool Initialized = false;
        private static RabbitMQCommunicationService Communicator { get; set; }

        // needed for marshaling calls back to UI thread
        private static CoreDispatcher _uiThreadDispatcher;

        private static string OutgoingQueueName { get; set; }
        private static string IncomingQueueName { get; set; }

        private static string LauncherOutgoingQueueName { get; set; }

        private static Dictionary<int, string> Tasks { get; } = new Dictionary<int, string>();

        /// <summary>
        /// Dictionary of pairs (Task_id, Function_called_after_complete).
        /// Function should receive a string parameter which contains a message
        /// with info about task result (DONE or some error)
        /// </summary>
        private static Dictionary<int, Action<string>> TaskCompleteActions { get; } = new Dictionary<int, Action<string>>();


        public static readonly string DoneMessage = "DONE";
        private static HashSet<string> TaskCompleteMessages { get; } = new HashSet<string>()
        {
            DoneMessage, "BAD PARAMS AND DATA", "NO DATA", "BAD REQUEST", "LACK OF METHOD",
            "LACK OF FILE", "WRONG EXTENSION", "LACK OF PATH", "WRONG ID"
        };


        private static int TaskIdCntr = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiThreadDispatcher"></param>
        /// <param name="databaseFilePath"></param>
        /// <exception cref="BackendControllerInitializationException">
        /// Throws BackendControllerInitializationException if the controller fails to
        /// set provided databaseFilePath
        /// </exception>
        public static async Task Initialize(CoreDispatcher uiThreadDispatcher, string databaseFilePath)
        {
            if (string.IsNullOrEmpty(databaseFilePath))
            {
                throw new ArgumentException("message", nameof(databaseFilePath));
            }

            if (!databaseFilePath.Any() || !Path.HasExtension(databaseFilePath))
                throw new ArgumentException("Invalid parameter: databaseFilePath should contain a valid path.");

            OutgoingQueueName = "front";
            IncomingQueueName = "back";
            LauncherOutgoingQueueName = "launcher";

            Communicator = RabbitMQCommunicationService.Instance;
            _uiThreadDispatcher = uiThreadDispatcher ?? throw new ArgumentNullException(nameof(uiThreadDispatcher));
            Communicator.Initialize(uiThreadDispatcher);

            Communicator.DeclareOutgoingQueue(LauncherOutgoingQueueName);
            Communicator.DeclareOutgoingQueue(OutgoingQueueName);
            Communicator.DeclareIncomingQueue(IncomingQueueName);

            Communicator.MessageReceived += MessageReceiver;

            RunTask(TaskIdCntr, $"{TaskIdCntr} PATH {databaseFilePath}", PathSendCompleteHandler);
            TaskIdCntr++;
            await Task.Delay(500);
            if (!Initialized)
                throw new BackendControllerInitializationException();
        }

        private static void PathSendCompleteHandler(string result)
        {
            if (result == DoneMessage)
                Initialized = true;
        }

        private static void MessageReceiver(object sender, MessageReceivedEventArgs e)
        {
            if (e.QueueName != IncomingQueueName)
                return;

            int key = 0;
            var split = e.Message.Split('-');

            if (split.Length != 2)
                return;
                //throw new FormatException($"Message is of bad format. Should contain only one dash. Message: {e.Message}");

            bool isInteger = int.TryParse(split[0], out key);

            if (!isInteger)
                return;
            //throw new FormatException($"Message is of bad format. No number at the beginning. Message: {e.Message}");

            if (!Tasks.ContainsKey(key))
                return;

            if (!TaskCompleteMessages.Contains(split[1]))
                return;
            //throw new FormatException($"Message is of bad format. Contains invalid result message. Message: {e.Message}");

            Action<string> action = TaskCompleteActions[key];
            TaskCompleteActions.Remove(key);
            Tasks.Remove(key);
            action(split[1]);
        }

        private static void RunTask(int taskId, string message, Action<string> actionToCallAfterComplete)
        {
            Tasks.Add(taskId, message);
            TaskCompleteActions.Add(taskId, actionToCallAfterComplete);
            Communicator.Send(OutgoingQueueName, message);
        }

        public static int CompareImages(List<int> comparedImagesIds, Action<string> actionToCallAfterComplete)
        {
            if (comparedImagesIds is null)
            {
                throw new ArgumentNullException(nameof(comparedImagesIds));
            }

            if (actionToCallAfterComplete is null)
            {
                throw new ArgumentNullException(nameof(actionToCallAfterComplete));
            }

            if (comparedImagesIds.Count < 2)
            {
                throw new ArgumentOutOfRangeException("comparedImagesIds list count should be at least 2");
            }

            int taskId = TaskIdCntr++;
            string message = $"{taskId} COMPARE";
            foreach (int id in comparedImagesIds)
            {
                message += $" {id}";
            }

            RunTask(taskId, message, actionToCallAfterComplete);

            return taskId;
        }

        public static async Task TagImages(List<int> taggedImagesIds)
        {
            if (taggedImagesIds is null)
            {
                throw new ArgumentNullException(nameof(taggedImagesIds));
            }

            foreach (var item in taggedImagesIds)
            {
                await AddressTaggingService.TagImageAddressAsync(item);
            }
        }

        public static void SendCloseApp()
        {
            Communicator.Send(LauncherOutgoingQueueName, "closing");
        }
    }
}
