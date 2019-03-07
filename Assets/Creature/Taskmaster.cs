using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Taskmaster : MonoBehaviour
{
    internal List<TaskBase> Tasks = new List<TaskBase>();

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

    public static void ProcessQueue(Queue<TaskBase> queue)
    {
        if (queue == null || queue.Count == 0)
        {
            return;
        }

        var current = queue.Peek();

        if (current.Done())
        {
            queue.Dequeue();
        }
        else
        {
            current.Update();
        }
    }

    public static bool QueueComplete(Queue<TaskBase> queue)
    {
        return queue == null || queue.Count == 0;
    }

    public IEnumerable<TaskBase> GetTaskByOriginator(string originatorId)
    {
        return Tasks.Where(t => t.Originator == originatorId);
    }

    public TaskBase AddTask(TaskBase task, string originatorId)
    {
        task.Originator = originatorId;
        Tasks.Add(task);
        return task;
    }

    public TaskBase GetNextAvailableTask()
    {
        return Tasks.FirstOrDefault(t => t.CreatureId <= 0);
    }

    public TaskBase GetTask(Creature creature)
    {
        TaskBase task = null;

        if (creature.Data.Hunger > 50)
        {
            task = new Eat("Food");
            AddTask(task, creature.Data.GetGameId());
        }
        else if (creature.Data.Energy < 15)
        {
            task = new Sleep();
            AddTask(task, creature.Data.GetGameId());
        }
        else
        {
            task = GetNextAvailableTask();
            if (task == null)
            {
                if (Random.value > 0.6)
                {
                    var wanderCircle = MapGrid.Instance.GetCircle(creature.Data.CurrentCell, 3).Where(c => c.TravelCost == 1).ToList();
                    if (wanderCircle.Count > 0)
                    {
                        task = new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)].Coordinates, (int)creature.Data.Speed / 3);
                    }
                }

                if (task == null)
                {
                    task = new Wait(Random.Range(0.1f, 1f), "Chilling");
                }

                AddTask(task, creature.Data.GetGameId());
            }
        }
        task.CreatureId = creature.Data.Id;
        return task;
    }

    internal bool ContainsJob(string name)
    {
        return true;
    }

    internal void TaskComplete(TaskBase task)
    {
        Tasks.Remove(task);
    }
}
