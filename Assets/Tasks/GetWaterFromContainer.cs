using Assets.Creature;
using Assets.ServiceLocator;
using Assets.Structures;
using System.Linq;

public class GetWaterFromContainer : CreatureTask
{
    public string ContainerId;

    public GetWaterFromContainer()
    {
    }

    public GetWaterFromContainer(LiquidContainer container) : this()
    {
        ContainerId = container.Id;
    }

    public override string Message { get; }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            if (string.IsNullOrEmpty(ContainerId))
            {
                FindContainer(creature);
            }

            var container = ContainerId.GetStructure() as LiquidContainer;
            if (!creature.InRangeOf(container))
            {
                AddSubTask(new Move(container.GetWorkCell()));
                return false;
            }

            var water = Loc.GetItemController().SpawnItem("StoredWater", creature.Cell, 1);
            creature.PickUpItem(water, 1);
            container.FillLevel -= 1;
            return true;
        }
        return false;
    }

    private void FindContainer(CreatureData creature)
    {
        var containers = GetWater.GetFactionWaterContainersWithWater(creature).OrderBy(l => l.Cell.DistanceTo(creature.Cell)).ToList();
        if (containers.Count > 0)
        {
            ContainerId = containers[0].Id;
        }
        else
        {
            throw new TaskFailedException("No container with water in can be found!");
        }
    }

    public override void FinalizeTask()
    {
    }
}