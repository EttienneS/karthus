﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Faction
{
    public List<CreatureTask> AvailableTasks = new List<CreatureTask>();

    public List<Creature> Creatures = new List<Creature>();
    public string FactionName;
    public List<Item> Items = new List<Item>();
    public float LastUpdate;
    public List<Structure> Structures = new List<Structure>();

    [JsonIgnore]
    public List<Cell> HomeCells = new List<Cell>();

    public float UpdateTick = 1;

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
            foreach (var availableTask in AvailableTasks.Where(t => creature.CanDo(t)))
            {
                if (creature.CanDo(availableTask))
                {
                    if (!creature.ManaPool.HasMana(availableTask.TotalCost.Mana))
                    {
                        Debug.Log($"Not enough mana available for task: {availableTask}");
                        continue;
                    }
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
        if (!Game.Ready)
            return;
        if (Game.TimeManager.Paused)
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
                            if (!item.InUseByAnyone && !item.InContainer && !storageTasks.Any(t => t.ItemToStoreId == item.Id))
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
            }
            

            foreach (var creature in Creatures)
            {
                // lost
                if (creature.Cell == null)
                {
                    creature.Cell = Game.Map.Center;
                }
            }
        }
    }

    public Structure GetStorageFor(Item item)
    {
        var pendingStorage = AvailableTasks.OfType<StoreItem>().ToList();
        var options = new List<Structure>();
        foreach (var structure in Structures.Where(s => !s.IsBluePrint && s.IsContainer()))
        {
            if (pendingStorage.Any(p => p.StorageStructureId == structure.Id))
            {
                // already allocated skip structure
                continue;
            }

            if (structure.CanHold(item))
            {
                options.Add(structure);
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

        HomeCells.AddRange(Game.Map.GetCircle(structure.Cell, 5));
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
}