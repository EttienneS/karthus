using System.Collections.Generic;

public class MoveItemToCell : TaskBase
{
    public bool Reserve;

    public MoveItemToCell()
    {
    }

    public MoveItemToCell(string itemType, Coordinates coordinates, bool allowStockpiled, bool reserve)
    {
        Reserve = reserve;
        SubTasks.Enqueue(new GetItemOfType(itemType, allowStockpiled));
        SubTasks.Enqueue(new Move(coordinates));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            var item = Creature.DropItem();
            if (Reserve)
            {
                item.Reserved = true;
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