using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class Faction
{
    public const int RecyleCount = 5;
    public const int RecyleTime = 3;
    public List<CreatureTask> AvailableTasks = new List<CreatureTask>();

    public Structure Core;

    public List<CreatureData> Creatures = new List<CreatureData>();
    public string FactionName;
    public int LastRecyle;
    public List<Structure> Structures = new List<Structure>();

    public CreatureTask AddTask(CreatureTask task)
    {
        AvailableTasks.Add(task);
        return task;
    }

    public CreatureTask TakeTask(CreatureData creature)
    {
        var task = creature.GetBehaviourTask?.Invoke(creature);
        if (task == null)
        {
            var highestPriority = int.MinValue;
            foreach (var availableTask in AvailableTasks.Where(t => creature.CanDo(t)))
            {
                var priority = creature.GetPriority(availableTask);
                if (priority > highestPriority)
                {
                    task = availableTask;
                    highestPriority = priority;
                }
            }
        }

        if (task != null)
        {
            AvailableTasks.Remove(task);
            return task;
        }

        return new Idle(creature);
    }

    internal void AddCreature(CreatureData data)
    {
        Creatures.Add(data);
        data.FactionName = FactionName;
    }

    internal void AddStructure(Structure structure)
    {
        Structures.Add(structure);
        structure.FactionName = FactionName;
    }

    internal void RemoveTask(CreatureTask task)
    {
        if (AvailableTasks.Contains(task))
        {
            AvailableTasks.Remove(task);
        }

        task.Destroy();
    }
}