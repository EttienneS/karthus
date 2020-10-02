using Assets.Creature;
using Assets.ServiceLocator;
using System.Linq;
using UnityEngine;

public class Idle : CreatureTask
{
    public Idle()
    {
    }

    public Idle(CreatureData creature) : this()
    {
        if (Random.value > 0.7)
        {
            var wanderCircle = Loc.GetMap().GetCircle(creature.Cell, 3).Where(c => c.TravelCost > 0 && c.TravelCost < 10).ToList();
            wanderCircle.Remove(creature.Cell);
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle.GetRandomItem()));
            }
        }
        else
        {
            AddSubTask(new Wait(Random.Range(2, 4), "Wait", AnimationType.Idle));
        }
    }

    public override string Message
    {
        get
        {
            return "Idle...";
        }
    }

    public override bool Done(CreatureData Creature)
    {
        return SubTasksComplete(Creature);
    }

    public override void FinalizeTask()
    {
    }
}