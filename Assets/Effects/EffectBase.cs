﻿using Newtonsoft.Json;
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
                _assignedEntity = AssignedEntityId.GetEntity();
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
                _target = TargetId.GetEntity();
            }
            return _target;
        }
    }

    public string TargetId;

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

        if (AssignedEntity is Creature creature &&
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