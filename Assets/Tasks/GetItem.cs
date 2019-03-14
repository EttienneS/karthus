public class GetItem : TaskBase
{
    public bool AllowStockpiled;
    public ItemData Item;
    public string ItemId;

    public SearchBy Search;

    public enum SearchBy
    {
        Name, Category
    }

    public GetItem()
    {
    }

    public GetItem(string item, bool allowStockpiled, SearchBy search)
    {
        AllowStockpiled = allowStockpiled;
        ItemId = item;
        Search = search;

        Message = $"Getting {item}";
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
            if (Search == SearchBy.Category)
            {
                Item = ItemController.Instance.FindClosestItemOfType(Creature.CurrentCell, ItemId, AllowStockpiled);
            }
            else
            {
                Item = ItemController.Instance.FindClosestItemByName(Creature.CurrentCell, ItemId, AllowStockpiled);
            }

            if (Item == null)
            {
                throw new TaskFailedException($"Unable to find item: {ItemId}");
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