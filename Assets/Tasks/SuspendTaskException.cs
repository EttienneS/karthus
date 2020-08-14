using System;
using System.Runtime.Serialization;

[Serializable]
internal class SuspendTaskException : Exception
{
    public SuspendTaskException()
    {
    }

    public SuspendTaskException(string message) : base(message)
    {
    }

    public SuspendTaskException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SuspendTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}