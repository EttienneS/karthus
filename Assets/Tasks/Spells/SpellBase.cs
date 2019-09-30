using System.Collections.Generic;
using UnityEngine;

public abstract class SpellBase : EntityTask
{
    public Dictionary<ManaColor, int> ManaCost;

    public float CastTime = 0.5f;

    public float Elapsed;

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

        if (DoSpell())
        {
            AssignedEntity.ManaPool.BurnMana(ManaCost);
            return true;
        }

        return false;
    }

    public abstract bool DoSpell();
}