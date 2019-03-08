using System;


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
                SubTasks.Enqueue(new MoveItemToCell(item, Location, true, true));
            }
        }

        SubTasks.Enqueue(new Wait(3f, "Crafting"));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            var location = MapGrid.Instance.GetCellAtCoordinate(Location);
            foreach (var item in location.ContainedItems.ToArray())
            {
                ItemController.Instance.DestroyItem(item);
            }
            location.AddContent(ItemController.Instance
                    .GetItem(OutputItemType).gameObject);

            return true;
        }
        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}