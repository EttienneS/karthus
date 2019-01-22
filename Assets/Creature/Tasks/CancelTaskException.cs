using System;
using System.Runtime.Serialization;

[Serializable]
internal class CancelTaskException : Exception
{
    public CancelTaskException()
    {
    }

    public CancelTaskException(string message) : base(message)
    {
    }

    public CancelTaskException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected CancelTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}