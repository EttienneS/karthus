using Assets.Item;
using System;
using System.Runtime.Serialization;

[Serializable]
internal class ItemNotAllowedInStoreException : Exception
{
    public ItemNotAllowedInStoreException(ItemData item) : base(item.ToString())
    {

    }


    public ItemNotAllowedInStoreException()
    {
    }

    public ItemNotAllowedInStoreException(string message) : base(message)
    {
    }

    public ItemNotAllowedInStoreException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ItemNotAllowedInStoreException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}