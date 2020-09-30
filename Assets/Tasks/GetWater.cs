using Assets.Creature;
using Assets.Structures;
using System.Collections.Generic;
using System.Linq;

internal class GetWater : CreatureTask
{
    public override string Message { get; }

    public static bool FactionHasWaterContainerWithWater(CreatureData creature)
    {
        return GetFactionWaterContainersWithWater(creature).Any();
    }

    public static IEnumerable<LiquidContainer> GetFactionWaterContainersWithWater(CreatureData creature)
    {
        return creature.Faction.Structures.OfType<LiquidContainer>()
                                          .Where(l => l.FillLevel > 0);
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            if (creature.HeldItem?.IsType("Water") == true)
            {
                return true;
            }
            else
            {
                if (FactionHasWaterContainerWithWater(creature))
                {
                    AddSubTask(new GetWaterFromContainer());
                }
                else
                {
                    AddSubTask(new GetWaterFromSource());
                }
                return false;
            }
        }

        return false;
    }

    public override void FinalizeTask()
    {
    }
}