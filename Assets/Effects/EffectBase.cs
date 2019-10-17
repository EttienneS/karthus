using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;


public abstract class EffectBase
{
    private IEntity _assignedEntity;

    [JsonIgnore]
    public IEntity AssignedEntity
    {
        get
        {
            if (_assignedEntity == null)
            {
                _assignedEntity = IdService.GetEntity(AssignedEntityId);
            }
            return _assignedEntity;
        }
    }

    public string AssignedEntityId;

    private IEntity _target;

    [JsonIgnore]
    public IEntity Target
    {
        get
        {
            if (_target == null)
            {
                _target = IdService.GetEntity(TargetId);
            }
            return _target;
        }
    }

    public string TargetId;

    public Dictionary<ManaColor, int> Cost;
    public float ActivationTime = 0.5f;
    public float Elapsed;

    public string DisplayName;

    public int Range;

    public bool Disabled;

    public bool Done()
    {
        if (Elapsed < ActivationTime)
        {
            Elapsed += Time.deltaTime;
            return false;
        }
        Elapsed = 0f;

        if (Cost != null && !AssignedEntity.ManaPool.HasMana(Cost))
        {
            if (AssignedEntity is CreatureData creature)
            {
                creature.Task.AddSubTask(new Acrue(Cost));
            }
            return false;
        }

        if (!Ready())
        {
            return false;
        }

        if (!InRange())
        {
            return false;
        }

        if (DoEffect())
        {
            if (Cost != null)
            {
                AssignedEntity.ManaPool.BurnMana(Cost);
            }
            return true;
        }

        return false;
    }

    public virtual bool InRange()
    {
        if (Range < 0)
        {
            return true;
        }

        if (AssignedEntity is CreatureData creature &&
            AssignedEntity.Cell.DistanceTo(Target.Cell) > Range)
        {
            var cell = Target.Cell;
            if (Range == 1)
            {
                cell = Target.Cell.GetRandomNeighbor();
            }
            else if (Range > 1)
            {
                var inRangeCells = Game.Map.GetCircle(Target.Cell, Range);
                inRangeCells.Remove(Target.Cell);
                inRangeCells.Shuffle();
                cell = inRangeCells[0];
            }

            creature.Task.AddSubTask(new Move(cell));
            return false;
        }

        return true;
    }

    public abstract bool DoEffect();

    public virtual bool Ready()
    {
        return true;
    }
}