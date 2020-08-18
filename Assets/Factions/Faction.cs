using Assets.Creature;
using Assets.Item;
using Assets.Structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction
{
    public List<CreatureTask> AvailableTasks = new List<CreatureTask>();

    public Dictionary<CreatureTask, CreatureData> AssignedTasks
    {
        get
        {
            return Creatures.Where(c => c.Task != null).ToDictionary(t => t.Task, c => c);
        }
    }

    public List<CreatureTask> AllTasks
    {
        get
        {
            var allTasks = AvailableTasks.ToList();
            allTasks.AddRange(AssignedTasks.Keys.ToList());

            return allTasks;
        }
    }

    public List<CreatureData> Creatures = new List<CreatureData>();
    public List<Blueprint> Blueprints = new List<Blueprint>();
    public string FactionName;
    public float LastUpdate;
    public float ResumeDelta;
    public List<Structure> Structures = new List<Structure>();

    [JsonIgnore]
    public List<Cell> HomeCells = new List<Cell>();

    public float UpdateTick = 1;
    public float AutoResumeTime = 500;

    [JsonIgnore]
    public IEnumerable<StorageZone> StorageZones
    {
        get
        {
            return Game.Instance.ZoneController.StorageZones.Where(z => z.FactionName == FactionName);
        }
    }

    public CreatureTask AddTask(CreatureTask task)
    {
        AvailableTasks.Add(task);
        return task;
    }

    public CreatureTask TakeTask(CreatureData creature)
    {
        var task = creature.Behaviour.GetTask(creature);
        if (task == null)
        {
            var highestPriority = int.MinValue;
            foreach (var availableTask in AvailableTasks.Where(t => !t.IsSupended() && creature.CanDo(t)))
            {
                if (creature.CanDo(availableTask))
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
            }
        }

        return task ?? new Idle(creature);
    }

    public void Update()
    {
        if (Game.Instance.TimeManager.Paused)
        {
            return;
        }

        LastUpdate += Time.deltaTime;
        if (LastUpdate > UpdateTick)
        {
            LastUpdate = 0;

            foreach (var blueprint in Blueprints)
            {
                if (blueprint.AssociatedBuildTask == null)
                {
                    AddTask(new Build(blueprint));
                }
            }
        }

        ResumeDelta += Time.deltaTime;
        if (ResumeDelta > AutoResumeTime)
        {
            ResumeDelta = 0;
            foreach (var task in AvailableTasks.Where(t => t.IsSupended() && t.AutoResume))
            {
                task.Resume();
            }
        }
    }

    internal void AddBlueprint(Blueprint blueprint)
    {
        if (!Blueprints.Contains(blueprint))
        {
            Blueprints.Add(blueprint);
        }
    }

    internal void AddCreature(CreatureData data)
    {
        if (!Creatures.Contains(data))
        {
            Creatures.Add(data);
        }
        data.FactionName = FactionName;
    }

    internal void AddStructure(Structure structure)
    {
        if (!Structures.Contains(structure))
        {
            Structures.Add(structure);
        }
        structure.FactionName = FactionName;

        if (FactionName != FactionConstants.World)
        {
            HomeCells.AddRange(Map.Instance.GetCircle(structure.Cell, 5));
            HomeCells = HomeCells.Distinct().ToList();
        }
    }

    public void LoadHomeCells()
    {
        foreach (var structure in Structures)
        {
            HomeCells.AddRange(Map.Instance.GetCircle(structure.Cell, 5));
        }
        HomeCells = HomeCells.Distinct().ToList();
    }

    internal void RemoveTask(CreatureTask task)
    {
        if (task != null)
        {
            if (AvailableTasks.Contains(task))
            {
                AvailableTasks.Remove(task);
            }

            task.Destroy();
        }
    }

    public ItemData FindItem(string criteria, CreatureData creature)
    {
        var items = HomeCells.SelectMany(c => c?.Items.Where(item => item.IsType(criteria) && !item.InUseByAnyone)).ToList();
        items.AddRange(Game.Instance.IdService.ItemLookup.Values.Where(i => i.FactionName == FactionName && i.IsType(criteria)));

        ItemData targetItem = null;
        var bestDistance = float.MaxValue;

        foreach (var item in items)
        {
            var best = false;
            var cell = item.Cell;
            var distance = creature.Cell.DistanceTo(item.Cell);

            if (targetItem == null)
            {
                best = true;
            }
            else
            {
                best = distance > bestDistance;
            }

            if (best)
            {
                targetItem = item;
                bestDistance = distance;
            }
        }

        return targetItem;
    }
   }