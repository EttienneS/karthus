using Structures;
using System;
using System.Collections.Generic;
using System.Linq;

public class Build : CreatureTask
{
    public Structure TargetStructure;

    private int _waitCount = 0;

    public override void Complete()
    {
    }

    public Build()
    {
        RequiredSkill = SkillConstants.Build;
        RequiredSkillLevel = 1;
    }

    public Build(Structure structure) : this()
    {
        TargetStructure = structure;
    }

    public bool Built = false;

    public override string Message
    {
        get
        {
            return $"Building {TargetStructure.Name} at {TargetStructure.Cell}";
        }
    }

    public override bool Done(Creature creature)
    {
        if (TargetStructure == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete(creature))
        {
            if (!Clean()) return false;
            if (!HasItems()) return false;
            if (!InPosition(creature)) return false;
            if (!CellOpen()) return false;
            if (!BuildComplete(creature)) return false;

            FinishStructure(creature.GetFaction());

            return true;
        }
        return false;
    }

    private bool BuildComplete(Creature creature)
    {
        if (!Built)
        {
            creature.Face(TargetStructure.Cell);
            var time = TargetStructure.Cost.Items.Sum(i => i.Value) * 5;
            AddSubTask(new Wait(time, "Building", AnimationType.Interact));
            Built = true;
            return false;
        }
        return true;
    }

    private bool CellOpen()
    {
        if (TargetStructure.Cell.Creatures.Count > 0)
        {
            _waitCount++;

            if (_waitCount > 10)
            {
                throw new TaskFailedException("Cannot build, cell occupied");
            }
            AddSubTask(new Wait(1, "Cell occupied", AnimationType.Interact));
            return false;
        }
        return true;
    }

    private bool InPosition(Creature creature)
    {
        if (!creature.Cell.NonNullNeighbors.Contains(TargetStructure.Cell))
        {
            AddSubTask(new Move(TargetStructure.Cell.GetPathableNeighbour()));
            return false;
        }
        return true;
    }

    private bool HasItems()
    {
        var needed = GetNeededItems();

        if (needed.Count > 0)
        {
            foreach (var item in needed)
            {
                AddSubTask(new FindAndHaulItem(item.Key, item.Value, TargetStructure.Cell, TargetStructure));
            }
            return false;
        }
        return true;
    }

    private bool Clean()
    {
        var nonStructureItems = TargetStructure.Cell.Items.Where(i => i.InUseById != TargetStructure.Id);
        if (nonStructureItems.Any())
        {
            foreach (var item in nonStructureItems)
            {
                item.Free();
                AddSubTask(new Pickup(item));
                AddSubTask(new Drop(TargetStructure.Cell.GetPathableNeighbour()));
            }

            return false;
        }

        var structuresToClean = Game.Instance.IdService.StructureCellLookup[TargetStructure.Cell].Where(c => !c.Buildable);

        if (structuresToClean.Any())
        {
            foreach (var structureToClean in structuresToClean)
            {
                AddSubTask(new RemoveStructure(structureToClean));
            }
            return false;
        }
        return true;
    }

    public void FinishStructure(Faction faction)
    {
        TargetStructure.IsBlueprint = false;

        faction.AddStructure(TargetStructure);

        foreach (var item in TargetStructure.Cell.Items.ToList())
        {
            Game.Instance.ItemController.DestroyItem(item);
        }
    }

    public Dictionary<string, int> GetNeededItems()
    {
        var current = TargetStructure.Cell.Items.ToList();
        var desired = new Dictionary<string, int>();
        foreach (var item in TargetStructure.Cost.Items)
        {
            var desiredAmount = item.Value;
            foreach (var existing in current.Where(i => i.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
            {
                desiredAmount -= existing.Amount;
            }

            if (desiredAmount > 0)
            {
                desired.Add(item.Key, desiredAmount);
            }
        }

        return desired;
    }
}