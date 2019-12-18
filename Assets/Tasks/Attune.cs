using System.Collections.Generic;

public class Attune : CreatureTask
{
    public Attune()
    {
        RequiredSkill = "Arcana";
        RequiredSkillLevel = 1;
    }

    public Attune(Dictionary<ManaColor, float> desired) : this()
    {
        Desired = desired;
    }

    public Attune(ManaColor color, float desired) : this(new Dictionary<ManaColor, float> { { color, desired } })
    {
    }

    public Dictionary<ManaColor, float> Desired { get; set; }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            foreach (var mana in Desired)
            {
                if (!creature.ManaPool.HasMana(mana))
                {
                    var diff = mana.Value - creature.ManaPool.GetTotal(mana.Key);
                    Game.VisualEffectController.SpawnLightEffect(creature, creature.Vector, mana.Key.GetActualColor(), 3, 3, diff * 1000);
                    AddSubTask(new Wait(diff, "Attuning..."));
                    AddSubTask(new GainManaTask(mana.Key, diff));

                    return false;
                }
            }

            return true;
        }
        return false;
    }
}