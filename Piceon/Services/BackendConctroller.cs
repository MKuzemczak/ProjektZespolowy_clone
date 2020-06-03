using Piceon.Models;
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
        public enum TaskType : int
        {
            Initialize = 0,
            Compare = 1,
            Quality
        };

        public static bool Initialized = false;
        private static RabbitMQCommunicationService Communicator { get; set; }

        // needed for marshaling calls back to UI thread
        private static CoreDispatcher _uiThreadDispatcher;

        private static string OutgoingQueueName { get; set; }
        private static string IncomingQueueName { get; set; }

        private static string LauncherOutgoingQueueName { get; set; }

        private static Dictionary<int, ControllerTaskRequestMessage> Tasks { get; } = new Dictionary<int, ControllerTaskRequestMessage>();

        /// <summary>
        /// Dictionary of pairs (Task_id, Function_called_after_complete).
        /// Function should receive a string parameter which contains a message
        /// with info about task result (DONE or some error)
        /// </summary>
        private static Dictionary<int, Action<ControllerTaskResultMessage>> TaskCompleteActions { get; } =
            new Dictionary<int, Action<ControllerTaskResultMessage>>();


        public static readonly string DoneMessage = "DONE";
        private static HashSet<string> TaskResultMessages { get; } = new HashSet<string>()
        {
            DoneMessage, "ERR"
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

            try
            {
                Communicator.DeclareOutgoingQueue(LauncherOutgoingQueueName);
            }
            catch (QueueAlreadyExistsException) { }
            try
            {
                Communicator.DeclareOutgoingQueue(OutgoingQueueName);
            }
            catch (QueueAlreadyExistsException) { }
            try
            {
                Communicator.DeclareIncomingQueue(IncomingQueueName);
            }
            catch (QueueAlreadyExistsException) { }

            Communicator.MessageReceived += MessageReceiver;

            var helloMessage = new ControllerTaskRequestMessage()
            {
                taskid = TaskIdCntr++,
                type = (int)TaskType.Initialize
            };

            RunTask(helloMessage, PathSendCompleteHandler);
            await Task.Delay(500);
            if (!Initialized)
                throw new BackendControllerInitializationException();
        }

        private static void PathSendCompleteHandler(ControllerTaskResultMessage result)
        {
            if (result.result == DoneMessage)
                Initialized = true;
        }

        private static void MessageReceiver(object sender, MessageReceivedEventArgs e)
        {
            if (e.QueueName != IncomingQueueName)
                return;

            if (!ControllerTaskResultMessage.IsJsonValidMessage(e.Message))
                return;

            var message = ControllerTaskResultMessage.FromJson(e.Message);

            if (!Tasks.ContainsKey(message.taskid))
                return;

            if (!TaskResultMessages.Contains(message.result))
                return;
            //throw new FormatException($"Message is of bad format. Contains invalid result message. Message: {e.Message}");

            Action<ControllerTaskResultMessage> action = TaskCompleteActions[message.taskid];
            TaskCompleteActions.Remove(message.taskid);
            Tasks.Remove(message.taskid);
            action(message);
        }

        private static void RunTask(ControllerTaskRequestMessage message, Action<ControllerTaskResultMessage> actionToCallAfterComplete)
        {
            Tasks.Add(message.taskid, message);
            TaskCompleteActions.Add(message.taskid, actionToCallAfterComplete);
            Communicator.Send(OutgoingQueueName, message.ToJson());
        }

        public static int CompareImages(List<ImageItem> comparedImageItems, Action<ControllerTaskResultMessage> actionToCallAfterComplete)
        {
            if (comparedImageItems is null)
            {
                throw new ArgumentNullException(nameof(comparedImageItems));
            }

            if (actionToCallAfterComplete is null)
            {
                throw new ArgumentNullException(nameof(actionToCallAfterComplete));
            }

            if (comparedImageItems.Count < 2)
            {
                throw new ArgumentOutOfRangeException("comparedImagesIds list count should be at least 2");
            }

            var message = new ControllerTaskRequestMessage()
            {
                taskid = TaskIdCntr++,
                type = (int)TaskType.Compare
            };
            message.images.AddRange(comparedImageItems.
                Select(i =>
                {
                    return new List<string>() { i.DatabaseId.ToString(), i.FilePath };
                }).ToList());

            RunTask(message, actionToCallAfterComplete);

            return message.taskid;
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

        public static int CheckImagesQuality(List<ImageItem> imageItems, Action<ControllerTaskResultMessage> actionToCallAfterComplete)
        {
            if (imageItems is null)
            {
                throw new ArgumentNullException(nameof(imageItems));
            }

            if (actionToCallAfterComplete is null)
            {
                throw new ArgumentNullException(nameof(actionToCallAfterComplete));
            }

            var message = new ControllerTaskRequestMessage()
            {
                taskid = TaskIdCntr++,
                type = (int)TaskType.Quality
            };
            message.images.AddRange(imageItems.
                Select(i =>
                {
                    return new List<string>() { i.DatabaseId.ToString(), i.FilePath };
                }).ToList());

            RunTask(message, actionToCallAfterComplete);

            return message.taskid;
        }

        public static void SendCloseApp()
        {
            Communicator.Send(LauncherOutgoingQueueName, "closing");
        }
    }
}
