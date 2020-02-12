using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Container : Structure
{
    public int Count { get; set; }

    public string Filter { get; set; }
    public string ItemType { get; set; }
    public int Capacity { get; set; }
    public int Priority { get; set; }

    internal bool HasItemOfType(string type)
    {
        return GetContainedItemTemplate()?.IsType(type) == true;
    }

    public Item GetContainedItemTemplate()
    {
        if (string.IsNullOrEmpty(ItemType))
        {
            return null;
        }

        return Game.ItemController.ItemDataReference[ItemType];
    }

    public void ClearItem()
    {
        Count = 0;
        ItemType = string.Empty;
    }

    public bool FilterMatch(string name, string[] categories)
    {
        var filter = Helpers.WildcardToRegex(Filter);

        if (Regex.IsMatch(name, filter))
        {
            return true;
        }
        if (categories != null)
        {
            foreach (var cat in categories)
            {
                if (Regex.IsMatch(cat, filter))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool FilterValid()
    {
        if (string.IsNullOrEmpty(ItemType))
        {
            return true;
        }
        return FilterMatch(ItemType, GetContainedItemTemplate().Categories);
    }

    public int RemainingCapacity
    {
        get
        {
            return Mathf.Max(Capacity - Count, 0);
        }
    }

    public bool Empty
    {
        get
        {
            return string.IsNullOrEmpty(ItemType);
        }
    }

    internal bool AddItem(Item item)
    {
        item.Coords = (Cell.Vector.x, Cell.Vector.y);

        if (Empty)
        {
            ItemType = item.Name;
            Count = item.Amount;
            Game.ItemController.DestroyItem(item);
            ContainedItemEffect = Game.VisualEffectController.SpawnSpriteEffect(this, Vector, item.SpriteName, float.MaxValue);
            return true;
        }
        else
        {
            if (!ItemType.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (item.Amount < RemainingCapacity)
            {
                Count += item.Amount;
                Game.ItemController.DestroyItem(item);
                return true;
            }
            else
            {
                item.Amount -= RemainingCapacity;
                Count = Capacity;
                return false;
            }
        }
    }

    internal bool CanHold(Item item)
    {
        if (!Empty)
        {
            if (ItemType.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Count + item.Amount <= Capacity;
            }
            else
            {
                return false;
            }
        }

        return FilterMatch(item.Name, item.Categories);
    }

    internal Item GetItem(int amount)
    {
        if (Count <= amount)
        {
            amount = Count;
        }

        var item = Game.ItemController.SpawnItem(ItemType, Cell, amount);
        Count -= amount;

        if (Count <= 0)
        {
            ContainedItemEffect?.DestroySelf();
            ClearItem();
        }

        return item;
    }
}