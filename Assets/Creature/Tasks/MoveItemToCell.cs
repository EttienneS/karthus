using System.Collections.Generic;

public class MoveItemToCell : ITask
{
    public Queue<ITask> SubTasks { get; set; }
    public Creature Creature { get; set; }
    public string TaskId { get; set; }

    public MoveItemToCell(string itemType, Cell cell, bool allowStockpiled)
    {
        SubTasks = new Queue<ITask>();
        SubTasks.Enqueue(new GetItemOfType(itemType, allowStockpiled));
        SubTasks.Enqueue(new Move(cell));
    }

    public bool Done()
    {
        return Taskmaster.QueueComplete(SubTasks);
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}