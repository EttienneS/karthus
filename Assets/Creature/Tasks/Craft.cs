public class Craft : TaskBase
{
    public string OutputItemType;
    public string[] RequiredItemTypes;

    public Coordinates Location;

    public Craft()
    {
    }

    public Craft(string itemType, string[] requiredItems, Coordinates location)
    {
        OutputItemType = itemType;
        RequiredItemTypes = requiredItems;

        Location = location;

        foreach (var itemString in requiredItems)
        {
            foreach (var item in Helpers.ParseItemString(itemString))
            {
                AddSubTask(new MoveItemToCell(item, Location, true, true));
            }
        }

        AddSubTask(new Wait(3f, "Crafting"));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var item in Creature.Mind[Context][MemoryType.Item])
            {
                ItemController.Instance.DestroyItem(IdService.GetItemFromId(item));
            }

            var craftedItem = ItemController.Instance.GetItem(OutputItemType);
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