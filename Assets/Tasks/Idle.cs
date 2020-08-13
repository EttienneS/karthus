using System.Linq;
using UnityEngine;
using Assets.Creature;

public class Idle : CreatureTask
{
    public override string Message
    {
        get
        {
            return "Idle...";
        }
    }

    public Idle()
    {
    }

    public override void FinalizeTask()
    {
    }

    public Idle(CreatureData creature) : this()
    {
        if (Random.value > 0.7)
        {
            var wanderCircle = Map.Instance.GetCircle(creature.Cell, 3).Where(c => c.TravelCost == 1).ToList();
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)]));
            }
        }
        else
        {
            AddSubTask(new Wait(Random.Range(3, 10), "Wait"));
        }
    }

    public override bool Done(CreatureData Creature)
    {
        return SubTasksComplete(Creature);
    }
}