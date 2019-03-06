using System;


public class Craft : TaskBase
{
    public string OutputItemType;
    public string[] RequiredItemTypes;

    public Craft()
    {
    }

    public Craft(string itemType, string[] requiredItems)
    {
        OutputItemType = itemType;
        RequiredItemTypes = requiredItems;
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            ItemController.Instance.GetItem(OutputItemType);
            return true;
        }
        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}