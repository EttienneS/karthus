using Assets.Item;
using System;

public class StorageFilter
{
    public string Filter { get; set; }

    internal bool Allows(ItemData item)
    {
        throw new NotImplementedException();
    }
}
