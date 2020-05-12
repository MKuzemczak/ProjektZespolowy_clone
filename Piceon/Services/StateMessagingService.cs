using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

namespace Piceon.Services
{
    public class StateMessagingService
    {
        private static StateMessagingService m_oInstance = null;
        private static readonly object m_oPadLock = new object();

        private List<StateMessage> Messages = new List<StateMessage>();

        public event EventHandler<MostRecentStateMessageUpdatedEventArgs> MostRecentStateMessageUpdatedEvent;
        private event EventHandler<MessageTimeoutRequestedEventArgs> MessageTimeoutRequestedEvent;

        public static StateMessagingService Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (m_oInstance == null)
                    {
                        m_oInstance = new StateMessagingService();
                    }
                    return m_oInstance;
                }
            }
        }

        private StateMessagingService()
        {
            MessageTimeoutRequestedEvent += MessageTimeoutEventHandler;
        }

        public StateMessage SendLoadingMessage(string text, uint timeoutMilliseconds = 0)
        {
            var msg = new StateMessage(true, text);
            Messages.Add(msg);
            MostRecentStateMessageUpdatedEvent(this, new MostRecentStateMessageUpdatedEventArgs(msg));
            if (timeoutMilliseconds > 0)
                MessageTimeoutRequestedEvent(this, new MessageTimeoutRequestedEventArgs(timeoutMilliseconds, msg));
            return msg;
        }

        public StateMessage SendInfoMessage(string text, uint timeoutMilliseconds = 0)
        {
            var msg = new StateMessage(false, text);
            Messages.Add(msg);
            MostRecentStateMessageUpdatedEvent(this, new MostRecentStateMessageUpdatedEventArgs(msg));
            if (timeoutMilliseconds > 0)
                MessageTimeoutRequestedEvent(this, new MessageTimeoutRequestedEventArgs(timeoutMilliseconds, msg));
            return msg;
        }

        public StateMessage GetMostRecentMessage()
        {
            if (Messages.Count == 0)
                return null;
            return Messages.Last();
        }

        public void RemoveMessage(StateMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (Messages.Count == 0)
                return;

            bool invokeRecentChange = (message == Messages.Last());

            Messages.Remove(message);

            if (invokeRecentChange)
                MostRecentStateMessageUpdatedEvent(this, new MostRecentStateMessageUpdatedEventArgs(null));
        }

        private async void MessageTimeoutEventHandler(object sender, MessageTimeoutRequestedEventArgs e)
        {
            if (e.TimeoutMilliseconds > 0)
                await Task.Delay((int)e.TimeoutMilliseconds);

            RemoveMessage(e.Message);
        }

        class MessageTimeoutRequestedEventArgs : EventArgs
        {
            public uint TimeoutMilliseconds { get; }
            public StateMessage Message { get; }

            public MessageTimeoutRequestedEventArgs(uint timeoutMilliseconds, StateMessage message)
            {
                TimeoutMilliseconds = timeoutMilliseconds;
                Message = message;
            }
        }
    }
}
