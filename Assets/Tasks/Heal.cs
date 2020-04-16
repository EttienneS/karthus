using System.Collections.Generic;
using UnityEngine;

public class Heal : CreatureTask
{
    public override string Message
    {
        get
        {
            return $"Tend to wounds";
        }
    }

    public Heal()
    {
        RequiredSkill = SkillConstants.Healing;
        RequiredSkillLevel = 1;
    }

    public override void Complete()
    {
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var wound = creature.GetWorstWound();
            if (wound != null)
            {
                Game.Instance.VisualEffectController.SpawnLightEffect(creature, creature.Vector, ColorConstants.WhiteAccent, 2, 1, 1).Fades();

                wound.Treated = true;
                wound.HealRate /= 2;
            }

            return true;
        }
        return false;
    }
}