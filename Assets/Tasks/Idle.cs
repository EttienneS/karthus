using System.Linq;
using UnityEngine;

public class Idle : CreatureTask
{
    public Idle()
    {
    }

    public Idle(CreatureData creature)
    {
        if (Random.value > 0.6)
        {
            var wanderCircle = Game.Map.GetCircle(creature.Cell, 2).Where(c => c.Bound && c.TravelCost == 1).ToList();
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)], (int)creature.Speed));
            }
        }
        else
        {
            AddSubTask(new Wait(0.1f, "Wait"));
        }

        Message = "Idle...";
    }

    public override bool Done(CreatureData Creature)
    {
        return SubTasksComplete(Creature);
    }
}