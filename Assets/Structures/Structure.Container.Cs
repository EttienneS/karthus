using System.Text.RegularExpressions;
using UnityEngine;

public partial class Structure //.Container
{
    public void ClearItem()
    {
        SetItemCount(0);
        SetContainedItem(string.Empty);
    }

    public int GetCapacity()
    {
        return Mathf.FloorToInt(GetValue(NamedProperties.Capacity));

    }

    public string GetContainedItem()
    {
        return GetProperty(NamedProperties.ContainedItemType);

    }

    public int GetItemCount()
    {
        if (!string.IsNullOrEmpty(GetContainedItem()))
        {
            return Mathf.FloorToInt(GetValue(NamedProperties.ContainedItemCount));
        }
        return 0;
    }

    public bool IsContainer()
    {
        return IsType(NamedProperties.Container);
    }

    public void SetContainedItem(string itemType)
    {
        SetProperty(NamedProperties.ContainedItemType, itemType);

    }

    public int SetItemCount(int count)
    {
        SetValue(NamedProperties.ContainedItemCount, count);
        return count;
    }

    internal bool AddItem(Item item)
    {
        item.Coords = (Cell.Vector.x, Cell.Vector.y);

        if (IsContainer())
        {
            var capacity = GetCapacity();
            var containedType = GetContainedItem();
            var currentCount = GetItemCount();
            if (string.IsNullOrEmpty(containedType))
            {
                SetContainedItem(item.Name);
                SetItemCount(currentCount + item.Amount);
                Game.ItemController.DestroyItem(item);
                ContainedItemEffect = Game.VisualEffectController.SpawnSpriteEffect(this, Vector, item.SpriteName, float.MaxValue);
                return true;
            }
            else
            {
                if (containedType != item.Name)
                {
                    return false;
                }

                var remainingCapacity = Mathf.FloorToInt(capacity - currentCount);
                if (item.Amount < remainingCapacity)
                {
                    SetItemCount(currentCount + item.Amount);
                    Game.ItemController.DestroyItem(item);
                    return true;
                }
                else
                {
                    SetItemCount(capacity);
                    item.Amount -= remainingCapacity;
                    return false;
                }
            }
        }

        return true;
    }

    internal bool CanHold(Item item)
    {
        var capacity = GetCapacity();
        var containedItemType = GetContainedItem();
        var containedItemCount = GetItemCount();

        if (string.IsNullOrEmpty(containedItemType) || containedItemType == item.Name)
        {
            return containedItemCount + item.Amount <= capacity;
        }

        if (!string.IsNullOrEmpty(containedItemType) && containedItemType != item.Name)
        {
            return false;
        }

        var filter = Helpers.WildcardToRegex(GetProperty(NamedProperties.Filter));
        return Regex.IsMatch(item.Name, filter);
    }

    internal Item GetItem(string itemType, int amount)
    {
        var containedType = GetContainedItem();
        var currentCount = GetItemCount();

        if (containedType != itemType)
        {
            return null;
        }

        if (currentCount <= amount)
        {
            amount = currentCount;
        }

        var item = Game.ItemController.SpawnItem(itemType, Cell, amount);
        currentCount -= amount;

        if (currentCount > 0)
        {
            SetItemCount(currentCount);
        }
        else
        {
            ContainedItemEffect?.DestroySelf();
            ClearItem();
        }

        return item;
    }

    public int RemainingCapacity()
    {
        return Mathf.Max(GetCapacity() - GetItemCount(), 0);
    }
}