public class MoveItemToCell : TaskBase
{
    public bool Reserve;
    public Coordinates Coordinates;

    public MoveItemToCell()
    {
    }

    public MoveItemToCell(string itemId, Coordinates coordinates, bool allowStockpiled, bool reserve, GetItem.SearchBy search)
    {
        Reserve = reserve;
        Coordinates = coordinates;

        AddSubTask(new GetItem(itemId, allowStockpiled, search));
        AddSubTask(new Move(MapGrid.Instance.GetPathableNeighbour(coordinates)));
        Message = $"Moving {itemId} to {coordinates}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            var item = Creature.DropItem(Coordinates);
            if (item != null && Reserve)
            {
                item.Reserved = true;
            }
            return true;
        }

        return false;
    }

    
}