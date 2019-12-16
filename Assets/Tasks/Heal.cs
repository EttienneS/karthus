using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Heal : CreatureTask
{
    public override Dictionary<ManaColor, float> Cost => new Dictionary<ManaColor, float> { { ManaColor.White, 2 } };
    public Heal()
    {
        RequiredSkill = "Heal";
        RequiredSkillLevel = 1;
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var white = creature.ManaPool[ManaColor.White].Total;
            if (white < 2)
            {
                AddSubTask(new Acrue(new Dictionary<ManaColor, float> { { ManaColor.White, 2 } }));
                return false;
            }

            var wound = creature.GetWorstWound();
            if (wound != null)
            {
                Game.VisualEffectController.SpawnSpriteEffect(creature, creature.Vector, "heart_t", 1f).Fades();
                Game.VisualEffectController.SpawnLightEffect(creature, creature.Vector, Color.white, 2, 1, 1).Fades();

                creature.ManaPool.BurnMana(ManaColor.White, 2);
                wound.Treated = true;
                wound.HealRate /= 2;
            }

            return true;
        }
        return false;
    }
}