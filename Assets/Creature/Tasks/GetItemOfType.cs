using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GetItemOfType : TaskBase
{
    
    private ItemData _item;
    
    private Coordinates _itemLocation;

    public GetItemOfType(string itemType, bool allowStockpiled)
    {
        AllowStockpiled = allowStockpiled;
        ItemType = itemType;
        SubTasks = new Queue<TaskBase>();
    }


    public bool AllowStockpiled;
    public string ItemType;

    public override bool Done()
    {
        if (_item != null && Taskmaster.QueueComplete(SubTasks))
        {
            if (!string.IsNullOrEmpty(_item.StockpileId))
            {
                var pile = StockpileController.Instance.GetStockpile(_item.StockpileId);
                _item = pile.GetItem(_item);
            }

            Creature.CarriedItem = _item;
            return true;
        }

        return false;
    }

    public override void Update()
    {
        if (_item == null)
        {
            _item = ItemController.Instance.FindClosestItemOfType(Creature.CurrentCell.LinkedGameObject, ItemType, AllowStockpiled);

            if (_item == null)
            {
                throw new CancelTaskException($"Unable to find item: {ItemType}");
            }
            _item.Reserved = true;
            UpdateTargetItem();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }

    private void UpdateTargetItem()
    {
        SubTasks = new Queue<TaskBase>();

        var moveTask = new Move(_item.LinkedGameObject.Cell.Data.Coordinates)
        {
            Creature = Creature
        };
        SubTasks.Enqueue(moveTask);
    }
}