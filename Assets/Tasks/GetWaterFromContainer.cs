using Assets.Creature;
using Assets.ServiceLocator;
using Assets.Structures;

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
            var container = ContainerId.GetStructure() as LiquidContainer;
            if (!creature.InRangeOf(container))
            {
                AddSubTask(new Move(container.GetWorkCell()));
                return false;
            }

            var water = Loc.GetItemController().SpawnItem("Water", creature.Cell, 1);
            creature.PickUpItem(water, 1);
            container.FillLevel -= 1;
            return true;
        }
        return false;
    }

    public override void FinalizeTask()
    {
    }
}