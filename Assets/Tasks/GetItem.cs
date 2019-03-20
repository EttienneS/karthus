using System.Collections.Generic;

public class GetItem : TaskBase
{
    public bool AllowStockpiled;
    public ItemData Item;
    public string SearchItem;

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
        SearchItem = item;
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
                Item = ItemController.Instance.FindClosestItemOfType(Creature.CurrentCell, SearchItem, AllowStockpiled);

                if (Item == null && SearchItem == "Drink")
                {
                    Item = ItemController.Instance.GetItem(new ItemData()
                    {
                        Name = "Water",
                        Category = "Drink",
                        Properties = new Dictionary<string, string> { { "Quench", "50" } }
                    }).Data;

                    MapGrid.Instance
                        .GetCellAtCoordinate(MapGrid.Instance
                        .GetPathableNeighbour(MapGrid.Instance
                        .GetNearestCellOfType(Creature.Coordinates, CellType.Water, 20).Coordinates))
                        .AddContent(Item.LinkedGameObject.gameObject);
                }
            }
            else
            {
                Item = ItemController.Instance.FindClosestItemByName(Creature.CurrentCell, SearchItem, AllowStockpiled);
            }

            if (Item == null)
            {
                throw new TaskFailedException($"Unable to find item: {SearchItem}");
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