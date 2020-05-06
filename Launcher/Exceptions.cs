using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    [Serializable]
    public class QueueAlreadyExistsException : Exception
    {
        public QueueAlreadyExistsException() : base("A queue with such name already exists.") { }
        public QueueAlreadyExistsException(string message) : base(message) { }
        public QueueAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
        protected QueueAlreadyExistsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class QueueDoesntExistException : Exception
    {
        public QueueDoesntExistException() : base("The queue doesn't exist") { }
        public QueueDoesntExistException(string message) : base(message) { }
        public QueueDoesntExistException(string message, Exception inner) : base(message, inner) { }
        protected QueueDoesntExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class BackendControllerInitializationException : Exception
    {
        public BackendControllerInitializationException() : base("Failed to initialize BackendController") { }
        public BackendControllerInitializationException(string message) : base(message) { }
        public BackendControllerInitializationException(string message, Exception inner) : base(message, inner) { }
        protected BackendControllerInitializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class NotInitializedException : Exception
    {
        public NotInitializedException() { }
        public NotInitializedException(string message) : base(message) { }
        public NotInitializedException(string message, Exception inner) : base(message, inner) { }
        protected NotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
