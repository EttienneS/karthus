using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public const int RecyleCount = 5;
    public const int RecyleTime = 3;
    public int LastRecyle;
    internal string FactionName;
    internal List<EntityTask> Tasks = new List<EntityTask>();

    internal Structure Core;

    internal List<CreatureData> Creatures = new List<CreatureData>();

    internal List<Structure> Structures = new List<Structure>();

   

    internal void AddStructure(Structure structure)
    {
        Structures.Add(structure);
        structure.FactionName = FactionName;
    }

   

    public EntityTask AddTask(EntityTask task, IEntity originatorId, TaskComplete taskComplete = null)
    {
        task.Originator = originatorId;
        task.CompleteEvent = taskComplete;
        Tasks.Add(task);
        return task;
    }

    public EntityTask AddTaskWithEntityBadge(EntityTask task, IEntity originatorId, IEntity badgedEntity, string badgeIcon)
    {
        var badge = Game.EffectController.AddBadge(badgedEntity, badgeIcon);
        AddTask(task, originatorId, badge.Destroy);
        return task;
    }

    public EntityTask AddTaskWithCellBadge(EntityTask task, IEntity originatorId, Cell cell, string badgeIcon)
    {
        var badge = Game.EffectController.AddBadge(cell, badgeIcon);
        AddTask(task, originatorId, badge.Destroy);
        return task;
    }

    public void AssignTask(CreatureData creature, EntityTask task, IEntity originator = null)
    {
        task.AssignedEntity = creature;

        if (originator != null)
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

    public EntityTask GetNextAvailableTask()
    {
        EntityTask task = null;
        foreach (var availableTask in Tasks.Where(t => t.AssignedEntity == null && !t.Failed))
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
            //            structure.Reserve(creature.Data.Id);
            //        }
            //    }
            //}

            task = availableTask;
            break;
        }
        return task;
    }

    public EntityTask GetTask(CreatureData creature)
    {
        var task = creature.GetBehaviourTask?.Invoke(creature);
        if (task == null)
        {
            task = GetNextAvailableTask() ?? new Idle(creature);
        }

        task.AssignedEntity = creature;
        return task;
    }

    public IEnumerable<EntityTask> GetTaskByOriginator(IEntity originator)
    {
        return Tasks.Where(t => t.Originator == originator);
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

    internal void TaskComplete(EntityTask task)
    {
        task.CompleteEvent?.Invoke();
        Tasks.Remove(task);
    }

    internal void CancelTask(EntityTask task)
    {
        task.CompleteEvent?.Invoke();

        if (task.AssignedEntity != null)
        {
            task.AssignedEntity.Task = null;
            task.AssignedEntity = null;
        }

        Tasks.Remove(task);

    }
}