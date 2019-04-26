public class Craft : TaskBase
{
    public string OutputItemName;
    public string[] RequiredItemNames;
    public float CraftTime;

    public Coordinates Location;

    public Craft()
    {
    }

    public Craft(string itemName, string[] requiredItems, Coordinates location, float craftTime)
    {
        OutputItemName = itemName;
        RequiredItemNames = requiredItems;
        Location = location;
        CraftTime = craftTime;

        foreach (var itemString in requiredItems)
        {
            foreach (var item in Helpers.ParseItemString(itemString))
            {
                AddSubTask(new MoveItemToCell(item, Location, true, true, GetItem.SearchBy.Name));
            }
        }

        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(Location)));
        AddSubTask(new Wait(CraftTime, "Crafting"));

        Message = $"Making {OutputItemName} at {location}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (RequiredItemNames.Length > 0)
            {
                foreach (var item in Creature.Mind[Context][MemoryType.Item])
                {
                    Game.ItemController.DestroyItem(IdService.GetItemFromId(item));
                }
            }

            var craftedItem = Game.ItemController.GetItem(OutputItemName);
            Game.MapGrid.GetCellAtCoordinate(Location).AddContent(craftedItem.gameObject);

            Creature.UpdateMemory(Context, MemoryType.Craft, craftedItem.Data.GetGameId());

            if (IdService.IsStructure(Originator))
            {
                Creature.UpdateMemory(Context, MemoryType.Structure, Originator);
            }

            return true;
        }
        return false;
    }
}