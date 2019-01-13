using System;
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

    public void AddTask(ITask task)
    {
        Tasks[TaskStatus.Available].Add(task);
    }

    public ITask GetNextAvailableTask()
    {
        if (Tasks[TaskStatus.Available].Any())
        {
            return Tasks[TaskStatus.Available][0];
        }
        return null;
    }

    internal void TaskComplete(ITask task)
    {
        Tasks[TaskStatus.InProgress].Remove(task);
    }

    public void FlagTaskAsInprogress(ITask task)
    {
        Tasks[TaskStatus.Available].Remove(task);
        Tasks[TaskStatus.InProgress].Add(task);
    }

    public ITask GetTask(Creature creature)
    {
        var task = GetNextAvailableTask();
        if (task == null)
        {
            if (Random.value > 0.6)
            {
                var wanderCircle = MapGrid.Instance.GetCircle(creature.CurrentCell, 3).Where(c => c.TravelCost == 1).ToList();
                if (wanderCircle.Any())
                {
                    task = new MoveTask(wanderCircle[Random.Range(0, wanderCircle.Count - 1)]);
                }
                else
                {
                    task = new MoveTask(creature.CurrentCell);
                }
            }
            else
            {
                task = new WaitTask(Random.Range(0.1f, 1f));
            }

            AddTask(task);
        }

        FlagTaskAsInprogress(task);

        return task;
    }
}