using System.Collections.Generic;
using UnityEngine;

public abstract class EffectBase : EntityTask
{
    public Dictionary<ManaColor, int> ManaCost;
    public float CastTime = 0.5f;
    public float Elapsed;

    public IEntity Target;


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

    public abstract bool DoEffect();

    public virtual bool Ready()
    {
        return true;
    }


}