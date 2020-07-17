using System;
using System.Runtime.Serialization;

namespace Assets.Models
{
    [Serializable]
    internal class MeshNotFoundException : Exception
    {
        public MeshNotFoundException()
        {
        }

        public MeshNotFoundException(string message) : base(message)
        {
        }

        public MeshNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MeshNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}