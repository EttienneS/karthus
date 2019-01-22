using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Taskmaster : MonoBehaviour
{
    internal Dictionary<TaskStatus, List<ITask>> Tasks = new Dictionary<TaskStatus, List<ITask>>
    {
        { TaskStatus.Available, new List<ITask>() },
        { TaskStatus.InProgress, new List<ITask>() },
    };

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

    public static void ProcessQueue(Queue<ITask> queue)
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

    public static bool QueueComplete(Queue<ITask> queue)
    {
        return queue == null || queue.Count == 0;
    }

    public ITask AddTask(ITask task)
    {
        Tasks[TaskStatus.Available].Add(task);
        return task;
    }

    public void FlagTaskAsInprogress(ITask task)
    {
        Tasks[TaskStatus.Available].Remove(task);
        Tasks[TaskStatus.InProgress].Add(task);
    }

    public ITask GetNextAvailableTask()
    {
        if (Tasks[TaskStatus.Available].Any())
        {
            return Tasks[TaskStatus.Available][0];
        }
        return null;
    }

    public ITask GetTask(Creature creature)
    {
        var task = GetNextAvailableTask();
        if (task == null)
        {
            if (Random.value > 0.6)
            {
                var wanderCircle = MapGrid.Instance.GetCircle(creature.CurrentCell, 3).Where(c => c.TravelCost == 1).ToList();
                if (wanderCircle.Count > 0)
                {
                    task = new Move(wanderCircle[Random.Range(0, wanderCircle.Count - 1)], (int)creature.Speed / 3);
                }
            }

            if (task == null)
            {
                task = new Wait(Random.Range(0.1f, 1f));
            }

            AddTask(task);
        }

        FlagTaskAsInprogress(task);

        return task;
    }

    internal bool ContainsJob(string name)
    {
        return true;
    }

    internal void TaskComplete(ITask task)
    {
        Tasks[TaskStatus.InProgress].Remove(task);
    }
}