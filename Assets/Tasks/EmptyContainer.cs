using Assets.Creature;
using Assets.Structures;

public class EmptyContainer : CreatureTask
{
    public string ContainerId;

    public override string Message
    {
        get
        {
            return $"Empty container {ContainerId}";
        }
    }

    public EmptyContainer()
    {
    }

    public EmptyContainer(Structure container) : this()
    {
        ContainerId = container.Id;

        AddSubTask(new Move(container.Cell));
    }

    public override void Complete()
    {
    }

    public override bool Done(CreatureData creature)
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
                AddSubTask(new Drop(Game.Instance.Map.GetNearestEmptyCell(creature.Cell)));
            }
        }

        return false;
    }
}