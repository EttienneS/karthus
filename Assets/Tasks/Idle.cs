using System.Linq;
using UnityEngine;

public class Idle : CreatureTask
{
    public Idle()
    {
    }

    public override void Complete()
    {
    }

    public Idle(Creature creature) : this()
    {
        if (Random.value > 0.6)
        {
            var wanderCircle = Game.Instance.Map.GetCircle(creature.Cell, 2).Where(c => c.TravelCost == 1).ToList();
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)]));
            }
        }
        else
        {
            AddSubTask(new Wait(0.1f, "Wait"));
        }

        Message = "Idle...";
    }

    public override bool Done(Creature Creature)
    {
        return SubTasksComplete(Creature);
    }
}