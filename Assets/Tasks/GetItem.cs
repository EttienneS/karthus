using System.Collections.Generic;

public class GetItem : TaskBase
{
    public bool AllowStockpiled;
    public ItemData Item;
    public string SearchItem;

    public SearchBy Search;

    public enum SearchBy
    {
        Name, Category, Id
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
        if (Item != null && Faction.QueueComplete(SubTasks))
        {
            if (Item.StockpileId != 0)
            {
                var pile = Game.StockpileController.GetStockpile(Item.StockpileId);
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
                Item = Game.ItemController.FindClosestItemOfType(Creature.CurrentCell, SearchItem, AllowStockpiled);

                if (Item == null && SearchItem == "Drink")
                {
                    Item = Game.ItemController.GetItem(new ItemData()
                    {
                        Name = "Water",
                        Category = "Drink",
                        Properties = new Dictionary<string, string> { { "Quench", "50" } }
                    }).Data;

                    Game.MapGrid
                        .GetCellAtCoordinate(Game.MapGrid
                        .GetPathableNeighbour(Game.MapGrid
                        .GetNearestCellOfType(Creature.Coordinates, CellType.Water, 20).Coordinates))
                        .AddContent(Item.LinkedGameObject.gameObject);
                }
            }
            else if (Search == SearchBy.Id)
            {
                if (IdService.IsItem(SearchItem))
                {
                    Item = IdService.GetItemFromId(SearchItem);
                }
                else
                {
                    throw new TaskFailedException($"Search object is not an existing item: {SearchItem}");
                }
            }
            else
            {
                Item = Game.ItemController.FindClosestItemByName(Creature.CurrentCell, SearchItem, AllowStockpiled);
            }

            if (Item == null)
            {
                throw new TaskFailedException($"Unable to find item: {SearchItem}");
            }
            Item.Reserved = true;
            UpdateTargetItem();
        }

        Faction.ProcessQueue(SubTasks);
    }

    private void UpdateTargetItem()
    {
        AddSubTask(new Move(Item.LinkedGameObject.Cell.Coordinates));
    }
}