﻿public class MoveItemToCell : TaskBase
{
    public bool Reserve;

    public MoveItemToCell()
    {
    }

    public MoveItemToCell(string itemType, Coordinates coordinates, bool allowStockpiled, bool reserve)
    {
        Reserve = reserve;
        AddSubTask(new GetItemOfType(itemType, allowStockpiled));
        AddSubTask(new Move(coordinates));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            var item = Creature.DropItem();
            if (item != null && Reserve)
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