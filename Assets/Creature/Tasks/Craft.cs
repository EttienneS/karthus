using System;


public class Craft : TaskBase
{
    public string ItemType;

    public Craft()
    {
    }

    public Craft(string itemType)
    {
        ItemType = itemType;
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            ItemController.Instance.GetItem(ItemType);
            return true;
        }
        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}