using System;
using System.Collections.Generic;
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
        private static HashSet<string> TaskCompleteMessages { get; } = new HashSet<string>()
        {
            "DONE", "BAD PARAMS AND DATA", "NO DATA", "BAD REQUEST", "LACK OF METHOD",
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
        public static void Initialize(CoreDispatcher uiThreadDispatcher, string databaseFilePath)
        {
            if (!databaseFilePath.Any())
                throw new ArgumentException("Invalid parameter: databaseFilePath.");

            OutgoingQueueName = "front";
            IncomingQueueName = "back";
            LauncherOutgoingQueueName = "launcher";

            Communicator = RabbitMQCommunicationService.Instance;
            _uiThreadDispatcher = uiThreadDispatcher;
            Communicator.Initialize(uiThreadDispatcher);

            Communicator.DeclareOutgoingQueue(LauncherOutgoingQueueName);
            Communicator.DeclareOutgoingQueue(OutgoingQueueName);
            Communicator.DeclareIncomingQueue(IncomingQueueName);

            Communicator.MessageReceived += MessageReceiver;

            RunTask(TaskIdCntr++, $"PATH {databaseFilePath}",
                (result) =>
                {
                    if (result == "DONE")
                        Initialized = true;
                });
            Thread.Sleep(500);
            if (!Initialized)
                throw new BackendControllerInitializationException();
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

        public static int CompareImages(int imageIdToCompareTo, List<int> comparedImagesIds, Action<string> actionToCallAfterComplete)
        {
            int taskId = TaskIdCntr++;
            string message = $"{taskId} COMPARE {imageIdToCompareTo}";
            foreach (int id in comparedImagesIds)
            {
                message += $" {id}";
            }

            RunTask(taskId, message, actionToCallAfterComplete);

            return taskId;
        }

        public static void SendCloseApp()
        {
            Communicator.Send(LauncherOutgoingQueueName, "closing");
        }
    }


    [Serializable]
    public class BackendControllerInitializationException : Exception
    {
        public BackendControllerInitializationException() : base("Failed to initialize BackendController"){ }
        public BackendControllerInitializationException(string message) : base(message) { }
        public BackendControllerInitializationException(string message, Exception inner) : base(message, inner) { }
        protected BackendControllerInitializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
