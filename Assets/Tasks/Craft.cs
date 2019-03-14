public class Craft : TaskBase
{
    public string OutputItemName;
    public string[] RequiredItemNames;

    public Coordinates Location;

    public Craft()
    {
    }

    public Craft(string itemName, string[] requiredItems, Coordinates location)
    {
        OutputItemName = itemName;
        RequiredItemNames = requiredItems;

        Location = location;

        foreach (var itemString in requiredItems)
        {
            foreach (var item in Helpers.ParseItemString(itemString))
            {
                AddSubTask(new MoveItemToCell(item, Location, true, true, GetItem.SearchBy.Name));
            }
        }

        AddSubTask(new Wait(3f, "Crafting"));

        Message = $"Making {OutputItemName} at {location}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var item in Creature.Mind[Context][MemoryType.Item])
            {
                ItemController.Instance.DestroyItem(IdService.GetItemFromId(item));
            }

            var craftedItem = ItemController.Instance.GetItem(OutputItemName);
            MapGrid.Instance.GetCellAtCoordinate(Location).AddContent(craftedItem.gameObject);

            Creature.UpdateMemory(Context, MemoryType.Craft, craftedItem.Data.GetGameId());

            if (IdService.IsStructure(Originator))
            {
                Creature.UpdateMemory(Context, MemoryType.Structure, Originator);
            }

            return true;
        }
        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}