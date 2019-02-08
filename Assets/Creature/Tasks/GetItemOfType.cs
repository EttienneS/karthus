using System;
using System.Collections.Generic;

[Serializable]
public class GetItemOfType : TaskBase
{
    public bool AllowStockpiled;
    public ItemData Item;
    public string ItemType;

    public GetItemOfType()
    {
    }

    public GetItemOfType(string itemType, bool allowStockpiled)
    {
        AllowStockpiled = allowStockpiled;
        ItemType = itemType;
        SubTasks = new Queue<TaskBase>();
    }

    public override bool Done()
    {
        if (Item != null && Taskmaster.QueueComplete(SubTasks))
        {
            if (Item.StockpileId != 0)
            {
                var pile = StockpileController.Instance.GetStockpile(Item.StockpileId);
                Item = pile.GetItem(Item);
            }
            Creature.CarriedItemId = Item.Id;
            return true;
        }

        return false;
    }

    public override void Update()
    {
        if (Item == null)
        {
            Item = ItemController.Instance.FindClosestItemOfType(Creature.CurrentCell.LinkedGameObject, ItemType, AllowStockpiled);

            if (Item == null)
            {
                throw new CancelTaskException($"Unable to find item: {ItemType}");
            }
            Item.Reserved = true;
            UpdateTargetItem();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }

    private void UpdateTargetItem()
    {
        SubTasks = new Queue<TaskBase>();

        var moveTask = new Move(Item.LinkedGameObject.Cell.Data.Coordinates)
        {
            Creature = Creature
        };
        SubTasks.Enqueue(moveTask);
    }
}