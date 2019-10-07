using System.Collections.Generic;
using UnityEngine;

public abstract class EffectBase : EntityTask
{
    public Dictionary<ManaColor, int> ManaCost;
    public float ActivationTime = 0.5f;
    public float Elapsed;

    public IEntity Target;
    public string DisplayName;

    public int Range;

    public override bool Done()
    {
        if (Elapsed < ActivationTime)
        {
            Elapsed += Time.deltaTime;
            return false;
        }
        Elapsed = 0f;

        if (ManaCost != null && !AssignedEntity.ManaPool.HasMana(ManaCost))
        {
            if (AssignedEntity is CreatureData creature)
            {
                creature.Task.AddSubTask(new Acrue(ManaCost));
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
            if (ManaCost != null)
            {
                AssignedEntity.ManaPool.BurnMana(ManaCost);
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
            if (Range > 0)
            {
                var inRangeCells = Game.Map.GetCircle(Target.Cell, Range - 1);
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