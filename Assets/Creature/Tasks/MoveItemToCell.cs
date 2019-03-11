public class MoveItemToCell : TaskBase
{
    public bool Reserve;

    public MoveItemToCell()
    {
    }

    public MoveItemToCell(string itemId, Coordinates coordinates, bool allowStockpiled, bool reserve, GetItem.SearchBy search)
    {
        Reserve = reserve;
        AddSubTask(new GetItem(itemId, allowStockpiled, search));
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