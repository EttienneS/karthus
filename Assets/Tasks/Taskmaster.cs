using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public int LastRecyle;

    public const int RecyleTime = 3;
    public const int RecyleCount = 5;

    public void Update()
    {
        if (TimeManager.Instance.Data.Hour - LastRecyle > RecyleTime)
        {
            LastRecyle = TimeManager.Instance.Data.Hour;

            var failedTasks = Tasks.Where(t => t.Failed);

            foreach (var task in failedTasks.Take(RecyleCount))
            {
                task.Failed = false;
            }
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

    public TaskBase GetNextAvailableTask(Creature creature)
    {
        TaskBase task = null;
        foreach (var availableTask in Tasks.Where(t => t.AssignedCreatureId <= 0 && !t.Failed))
        {
            var craftTask = availableTask as Craft;
            if (craftTask != null)
            {
                if (IdService.IsStructure(craftTask.Originator))
                {
                    var structure = IdService.GetStructureFromId(craftTask.Originator);

                    if (structure.InUseByAnyone)
                    {
                        continue;
                    }
                    else
                    {
                        structure.Reserve(creature.Data.GetGameId());
                    }
                }
            }

            task = availableTask;
            break;
        }
        return task;
    }

    public TaskBase GetTask(Creature creature)
    {
        TaskBase task;

        if (creature.Data.Hunger > 50)
        {
            task = AddTask(new Eat("Food"), creature.Data.GetGameId());
        }
        else if (creature.Data.Energy < 15)
        {
            task = AddTask(new Sleep(), creature.Data.GetGameId());
        }
        else
        {
            task = GetNextAvailableTask(creature);
            if (task == null)
            {
                task = AddTask(new Idle(creature.Data), creature.Data.GetGameId());
            }
        }
        task.AssignedCreatureId = creature.Data.Id;

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

    internal void TaskFailed(TaskBase task, string reason)
    {
        task.Failed = true;

        if (task.AssignedCreatureId > 0)
        {
            task.Creature.Task = null;
            if (task.Creature.CarriedItemId > 0)
            {
                task.Creature.CarriedItem.Reserved = false;
                task.Creature.CarriedItemId = 0;
            }
        }

        task.Message += $"\n{reason}";
        task.AssignedCreatureId = -1;

        // move task to bottom of the list
        Tasks.Remove(task);
        Tasks.Add(task);
    }
}