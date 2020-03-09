using Structures;
public class EmptyContainer : CreatureTask
{
    public string ContainerId;

    public EmptyContainer()
    {
    }

    public EmptyContainer(Structure container) : this()
    {
        ContainerId = container.Id;

        AddSubTask(new Move(container.Cell));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var container = ContainerId.GetContainer();

            if (string.IsNullOrEmpty(container.ItemType))
            {
                return true;
            }
            else
            {
                var item = container.GetItem(container.Count);
                AddSubTask(new Pickup(item));
                AddSubTask(new Drop(Game.Map.GetNearestEmptyCell(creature.Cell), item));
            }
        }

        return false;
    }
}