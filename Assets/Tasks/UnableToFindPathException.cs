using System;
using System.Runtime.Serialization;

namespace Assets.Tasks
{
    [Serializable]
    internal class UnableToFindPathException : Exception
    {
        public UnableToFindPathException(Cell cell) : base($"Unable to reach cell: {cell}")
        {
        }

        public UnableToFindPathException()
        {
        }

        public UnableToFindPathException(string message) : base(message)
        {
        }

        public UnableToFindPathException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnableToFindPathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}