using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Piceon.Services
{
    public sealed class RabbitMQCommunicationService
    {
        public bool Initialized = false;

        private static RabbitMQCommunicationService m_oInstance = null;
        private static readonly object m_oPadLock = new object();

        private ConnectionFactory Factory { get; set; }
        private IConnection Connection { get; set; }
        private IModel ConnectionModel { get; set; }

        private List<string> Queues = new List<string>();

        // needed for marshaling calls back to UI thread
        private CoreDispatcher _uiThreadDispatcher;

        private string CurrentReceivedQueue { get; set; }
        private string CurrentReceivedMessage { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public static RabbitMQCommunicationService Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (m_oInstance == null)
                    {
                        m_oInstance = new RabbitMQCommunicationService();
                    }
                    return m_oInstance;
                }
            }
        }

        private RabbitMQCommunicationService()
        {
        }

        public void Initialize(CoreDispatcher uiThreadDispatcher)
        {
            _uiThreadDispatcher = uiThreadDispatcher ?? throw new ArgumentNullException(nameof(uiThreadDispatcher));

            Factory = new ConnectionFactory() { HostName = "localhost" };
            Connection = Factory.CreateConnection();
            ConnectionModel = Connection.CreateModel();
            Initialized = true;
        }

        public void DeclareOutgoingQueue(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Queue name should consist of non-whitespace characters", nameof(name));
            }

            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            if (Queues.Contains(name))
            {
                throw new QueueAlreadyExistsException(name);
            }

            ConnectionModel.QueueDeclare(queue: name,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Queues.Add(name);
            CleanQueue(name);
        }

        public void DeclareIncomingQueue(string name)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            if (Queues.Contains(name))
            {
                throw new QueueAlreadyExistsException(name);
            }

            ConnectionModel.QueueDeclare(queue: name,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Queues.Add(name);
            CleanQueue(name);

            var consumer = new EventingBasicConsumer(ConnectionModel);
            consumer.Received += Receiver;
            ConnectionModel.BasicConsume(queue: name,
                autoAck: true,
                consumer: consumer);

        }

        public void Send(string queue, string message)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            if (!Queues.Contains(queue))
            {
                throw new QueueDoesntExistException();
            }

            var body = Encoding.UTF8.GetBytes(message);

            ConnectionModel.BasicPublish(exchange: "",
                routingKey: queue,
                basicProperties: null,
                body: body);
        }

        private async void Receiver(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            CurrentReceivedMessage = Encoding.ASCII.GetString(body);
            CurrentReceivedQueue = ea.RoutingKey;
            await _uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, InvokeMessageReceivedEvent);
        }

        private void InvokeMessageReceivedEvent()
        {
            MessageReceived(this, new MessageReceivedEventArgs(CurrentReceivedQueue, CurrentReceivedMessage));
        }

        public void CleanQueue(string queueName)
        {
            ConnectionModel.QueuePurge(queueName);
        }

    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string QueueName { get; set; }
        public string Message { get; set; }

        public MessageReceivedEventArgs(string queueName, string message)
        {
            QueueName = queueName;
            Message = message;
        }
    }
}
