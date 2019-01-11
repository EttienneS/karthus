using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Taskmaster : MonoBehaviour
{
    internal List<ITask> Tasks = new List<ITask>();
    private static Taskmaster _instance;

    public static Taskmaster Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("Taskmaster").GetComponent<Taskmaster>();
            }

            return _instance;
        }
    }
    public void AddTask(ITask task)
    {

    }

    public ITask GetTask(Creature creature)
    {
        if (!Tasks.Any())
        {
            var wanderCircle = MapGrid.Instance.GetCircle(creature.CurrentCell, 3).Where(c => c.TravelCost == 1).ToList();

            if (wanderCircle.Any())
            {
                return new MoveTask(wanderCircle[Random.Range(0, wanderCircle.Count() - 1)]);
            }
            else
            {
                return new MoveTask(creature.CurrentCell);
            }
        }

        return Tasks[0];
    }
}
