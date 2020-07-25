using Assets.Item;
using System;
using System.Runtime.Serialization;

[Serializable]
internal class NoCellFoundException : Exception
{
    public NoCellFoundException()
    {
    }

    public NoCellFoundException(string message) : base(message)
    {
    }

    public NoCellFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NoCellFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}