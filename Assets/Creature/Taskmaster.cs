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

    public TaskBase GetNextAvailableTask(Creature creature)
    {
        TaskBase task = null;
        foreach (var availableTask in Tasks.Where(t => t.AssignedCreatureId <= 0))
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
        TaskBase task = null;

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
}