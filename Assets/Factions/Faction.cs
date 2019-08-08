using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public const int RecyleCount = 5;
    public const int RecyleTime = 3;
    public int LastRecyle;
    internal string FactionName;
    internal List<TaskBase> Tasks = new List<TaskBase>();

    internal Structure Core;

    internal List<CreatureData> Creatures = new List<CreatureData>();

    internal List<Structure> Structures = new List<Structure>();

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
    }

    internal void AddStructure(Structure structure)
    {
        Structures.Add(structure);
        structure.FactionName = FactionName;
    }

    public static bool QueueComplete(Queue<TaskBase> queue)
    {
        if (queue == null || queue.Count == 0)
        {
            return true;
        }
        ProcessQueue(queue);
        return false;
    }

    public TaskBase AddTask(TaskBase task, string originatorId)
    {
        task.Originator = originatorId;
        Tasks.Add(task);
        return task;
    }

    public void AssignTask(CreatureData creature, TaskBase task, string originator = "")
    {
        task.AssignedCreatureId = creature.Id;

        if (!string.IsNullOrEmpty(originator))
        {
            task.Originator = originator;
        }

        if (task.SubTasks != null)
        {
            foreach (var subTask in task.SubTasks.ToList())
            {
                subTask.Context = task.Context;
                AssignTask(creature, subTask, task.Originator);
            }
        }
    }

    internal void AddCreature(CreatureData data)
    {
        Creatures.Add(data);
        data.FactionName = FactionName;
    }

    public TaskBase GetNextAvailableTask()
    {
        TaskBase task = null;
        foreach (var availableTask in Tasks.Where(t => t.AssignedCreatureId <= 0 && !t.Failed))
        {
            //var craftTask = availableTask as Craft;
            //if (craftTask != null)
            //{
            //    if (IdService.IsStructure(craftTask.Originator))
            //    {
            //        var structure = IdService.GetStructureFromId(craftTask.Originator);

            //        if (structure.InUseByAnyone)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            structure.Reserve(creature.Data.GetGameId());
            //        }
            //    }
            //}

            task = availableTask;
            break;
        }
        return task;
    }

    public TaskBase GetTask(CreatureData creature)
    {
        var task = creature.GetBehaviourTask?.Invoke(creature);
        if (task == null)
        {
            task = GetNextAvailableTask() ?? new Idle(creature);
        }

        task.AssignedCreatureId = creature.Id;
        return AddTask(task, creature.GetGameId());
    }

    public IEnumerable<TaskBase> GetTaskByOriginator(string originatorId)
    {
        return Tasks.Where(t => t.Originator == originatorId);
    }

    public void Update()
    {
        if (Game.TimeManager.Data.Hour - LastRecyle > RecyleTime)
        {
            LastRecyle = Game.TimeManager.Data.Hour;

            var failedTasks = Tasks.Where(t => t.Failed);

            foreach (var task in failedTasks.Take(RecyleCount))
            {
                task.Failed = false;
            }
        }
    }

    internal void TaskComplete(TaskBase task)
    {
        Tasks.Remove(task);
    }

    internal void TaskFailed(TaskBase task, string reason)
    {
        task.Failed = true;

        task.Message += $"\n{reason}";
        task.AssignedCreatureId = -1;

        // move task to bottom of the list
        Tasks.Remove(task);
        Tasks.Add(task);
    }
}