using System.Linq;
using UnityEngine;

public class Idle : TaskBase
{
    public Idle()
    {
    }

    public Idle(CreatureData creature)
    {
        if (Random.value > 0.6)
        {
            var wanderCircle = MapGrid.Instance.GetCircle(creature.CurrentCell, 5).Where(c => c.TravelCost == 1).ToList();
            if (wanderCircle.Count > 0)
            {
                AddSubTask(new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)].Coordinates, (int)creature.Speed / Random.Range(2, 6)));
            }
        }
        else
        {
            AddSubTask(new Wait(Random.Range(2f, 4f), "Chilling"));
        }
    }

    public override bool Done()
    {
        return Taskmaster.QueueComplete(SubTasks);
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}