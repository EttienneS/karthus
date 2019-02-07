using System.Collections.Generic;

public class MoveItemToCell : TaskBase
{
    public MoveItemToCell(string itemType, Coordinates coordinates, bool allowStockpiled)
    {
        SubTasks = new Queue<TaskBase>();
        SubTasks.Enqueue(new GetItemOfType(itemType, allowStockpiled));
        SubTasks.Enqueue(new Move(coordinates));
    }

    public override bool Done()
    {
        return Taskmaster.QueueComplete(SubTasks);
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}