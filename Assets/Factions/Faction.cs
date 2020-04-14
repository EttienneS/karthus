using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Structures;

public class Faction
{
    public List<CreatureTask> AvailableTasks = new List<CreatureTask>();
    public Dictionary<CreatureTask, Creature> AssignedTasks
    {
        get
        {
            return Creatures.Where(c => c.Task != null).ToDictionary(t => t.Task, c => c);
        }
    }

    public List<Creature> Creatures = new List<Creature>();
    public string FactionName;
    public float LastUpdate;
    public List<Structure> Structures = new List<Structure>();

    public IEnumerable<Container> Containers
    {
        get
        {
            return Structures.OfType<Container>();
        }
    }

    [JsonIgnore]
    public List<Cell> HomeCells = new List<Cell>();

    public float UpdateTick = 1;

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

    public CreatureTask TakeTask(Creature creature)
    {
        var task = creature.GetBehaviourTask?.Invoke(creature);
        if (task == null)
        {
            var highestPriority = int.MinValue;
            foreach (var availableTask in AvailableTasks.Where(t => !t.Suspended && creature.CanDo(t)))
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
        if (!Game.Instance.Ready)
            return;
        if (Game.Instance.TimeManager.Paused)
        {
            return;
        }

        LastUpdate += Time.deltaTime;
        if (LastUpdate > UpdateTick)
        {
            LastUpdate = 0;

            if (FactionName == FactionConstants.Player)
            {
                foreach (var structure in Structures.Where(s => s.IsBluePrint))
                {
                    if (!AvailableTasks.OfType<Build>().Any(t => t.TargetStructure == structure) &&
                        !Creatures.Any(t => t.Task is Build task && task.TargetStructure == structure))
                    {
                        AddTask(new Build(structure));
                    }
                }

                var storageTasks = AvailableTasks.OfType<StoreItem>().ToList();

                if (storageTasks.Count < 10)
                {
                    foreach (var cell in HomeCells)
                    {
                        foreach (var item in cell.Items)
                        {
                            if (!item.InUseByAnyone && !storageTasks.Any(t => t.ItemToStoreId == item.Id))
                            {
                                var storage = GetStorageFor(item);
                                if (storage != null)
                                {
                                    AddTask(new StoreItem(item, storage));
                                }
                            }
                        }
                    }
                }

                var emptyTasks = AvailableTasks.OfType<EmptyContainer>().ToList();
                foreach (var zone in Game.Instance.ZoneController.StorageZones.Where(z => z.FactionName == FactionName))
                {
                    foreach (var container in zone.Containers)
                    {
                        if (emptyTasks.Any(t => t.ContainerId == container.Id))
                        {
                            continue;
                        }
                        if (!container.FilterValid())
                        {
                            AddTask(new EmptyContainer(container));
                        }
                    }
                }
            }

            foreach (var creature in Creatures)
            {
                // lost
                if (creature.Cell == null)
                {
                    creature.Cell = Game.Instance.Map.Center;
                }
            }
        }
    }

    public Container GetStorageFor(Item item)
    {
        var pendingStorage = AvailableTasks.OfType<StoreItem>().ToList();
        var options = new List<Container>();
        foreach (var container in Containers.Where(s => !s.IsBluePrint))
        {
            if (pendingStorage.Any(p => p.StorageStructureId == container.Id))
            {
                // already allocated skip structure
                continue;
            }

            if (container.CanHold(item))
            {
                options.Add(container);
            }
        }

        if (options.Count > 0)
        {
            return options.OrderBy(n => n.Cell.DistanceTo(item.Cell)).First();
        }

        return null;
    }

    internal void AddCreature(Creature data)
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

        HomeCells.AddRange(Game.Instance.Map.GetCircle(structure.Cell, 5));
        HomeCells = HomeCells.Distinct().ToList();
    }

    public void LoadHomeCells()
    {
        foreach (var structure in Structures)
        {
            HomeCells.AddRange(Game.Instance.Map.GetCircle(structure.Cell, 5));
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

    public IEntity FindItemOrContainer(string criteria, Creature creature)
    {
        IEntity entity = FindContainerWithItem(criteria, creature);

        if (entity == null)
        {
            entity = FindItem(criteria, creature);
        }

        return entity;
    }

    public Item FindItem(string criteria, Creature creature)
    {
        var items = HomeCells.SelectMany(c => c?.Items.Where(item => item.IsType(criteria) && !item.InUseByAnyone)).ToList();
        items.AddRange(Game.Instance.IdService.ItemLookup.Values.Where(i => i.FactionName == FactionName && i.IsType(criteria)));

        Item targetItem = null;
        var bestDistance = float.MaxValue;

        foreach (var item in items)
        {
            var best = false;
            var cell = item.Cell;
            var distance = Pathfinder.Distance(creature.Cell, item.Cell, creature.Mobility);

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

    public Structure FindContainerWithItem(string criteria, Creature creature)
    {
        var containers = StorageZones.SelectMany(zone => zone.Containers.Where(container => container.HasItemOfType(criteria) && !container.InUseByAnyone));

        Structure targetContainer = null;
        var bestDistance = float.MaxValue;

        foreach (var container in containers)
        {
            var best = false;
            var cell = container.Cell;
            var distance = Pathfinder.Distance(creature.Cell, container.Cell, creature.Mobility);

            if (targetContainer == null)
            {
                best = true;
            }
            else
            {
                best = distance > bestDistance;
            }

            if (best)
            {
                targetContainer = container;
                bestDistance = distance;
            }
        }

        return targetContainer;
    }
}