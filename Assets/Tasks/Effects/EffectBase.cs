using System.Collections.Generic;
using UnityEngine;

public abstract class EffectBase : EntityTask
{
    public Dictionary<ManaColor, int> ManaCost;
    public float CastTime = 0.5f;
    public float Elapsed;

    public IEntity Target;

    public abstract int Range { get; }


    public override bool Done()
    {
        if (Elapsed < CastTime)
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

        if (AssignedEntity.Cell.DistanceTo(Target.Cell) > Range)
        {
            var spot = Game.Map.GetCircle(Target.Cell, Range - 1);
            spot.Shuffle();
            AssignedEntity.Task.AddSubTask(new Move(spot[0]));
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