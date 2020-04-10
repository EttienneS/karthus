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

        foreach (var item in structure.Cell.Items)
        {
            item.InUseById = null;
            AddSubTask(new Pickup(item));
            AddSubTask(new Drop(structure.Cell.GetPathableNeighbour()));
        }

        foreach (var item in structure.Cost.Items)
        {
            AddSubTask(new FindAndHaulItem(item.Key, item.Value, structure.Cell, structure));
        }
        AddSubTask(new Move(structure.Cell.GetPathableNeighbour()));

        Message = $"Building {structure.Name} at {structure.Cell}";
    }

    public bool Built = false;

    public override bool Done(Creature creature)
    {
        if (TargetStructure == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete(creature))
        {
            creature.Face(TargetStructure.Cell);
            if (TargetStructure.Cell.Creatures.Count > 0)
            {
                _waitCount++;

                if (_waitCount > 10)
                {
                    throw new TaskFailedException("Cannot build, cell occupied");
                }
                AddSubTask(new Wait(0.5f, "Cell occupied", LPC.Spritesheet.Generator.Interfaces.Animation.Slash));
                return false;
            }

            if (!Built)
            {
                var time = TargetStructure.Cost.Items.Sum(i => i.Value);
                AddSubTask(new Wait(time, "Building", LPC.Spritesheet.Generator.Interfaces.Animation.Thrust));
                Built = true;
                return false;
            }

            TargetStructure.IsBluePrint = false;
            TargetStructure.Refresh();

            if (TargetStructure.IsInterlocking())
            {
                foreach (var neighbour in TargetStructure.Cell.NonNullNeighbors)
                {
                    if (neighbour.Structure != null)
                    {
                        neighbour.Structure.Refresh();
                    }
                }
            }

            creature.GetFaction().AddStructure(TargetStructure);

            foreach (var item in GetContainedItems())
            {
                Game.ItemController.DestroyItem(item);
            }
            if (TargetStructure.Properties.ContainsKey(NamedProperties.ContainedItemIds))
            {
                TargetStructure.Properties.Remove(NamedProperties.ContainedItemIds);
            }

            if (TargetStructure.IsShadowCaster())
            {
                TargetStructure.Renderer.EnableShadow();
            }
            else
            {
                TargetStructure.Renderer.DisableShadow();
            }

            return true;
        }
        return false;
    }

    private List<Item> GetContainedItems()
    {
        if (TargetStructure.Properties.ContainsKey(NamedProperties.ContainedItemIds))
        {
            return TargetStructure.Properties[NamedProperties.ContainedItemIds]
                                  .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(i => i.GetItem()).ToList();
        }

        return new List<Item>();
    }
}