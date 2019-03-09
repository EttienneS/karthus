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
            Creature.UpdateMemory(Context, MemoryType.Item, Item.GetGameId());
            return true;
        }

        return false;
    }

    public override void Update()
    {
        if (Item == null)
        {
            Item = ItemController.Instance.FindClosestItemOfType(Creature.CurrentCell, ItemType, AllowStockpiled);

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
        AddSubTask(new Move(Item.LinkedGameObject.Cell.Coordinates));
    }
}