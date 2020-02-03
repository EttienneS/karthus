using UnityEngine;

public class Triage : BuffBase
{
    public Triage(string name) : base(name, cooldown: 1f, duration: 0.5f)
    {
    }

    public override int EstimateBuffEffect()
    {
        if (Owner.ManaPool[ManaColor.White].Total < 3)
        {
            // not enough mana!
            return int.MinValue;
        }

        var wound = Owner.GetWorstWound();
        if (wound == null)
        {
            return int.MinValue;
        }

        return (int)(wound.Danger * Owner.Aggression);
    }

    internal override void EndBuff()
    {
        Owner.Log($"{Owner.Name}'s triage spell takes effect");

        var wound = Owner.GetWorstWound();

        if (wound != null)
        {
            wound.Treated = true;
        }
        else
        {
            Owner.Log("The spell fizzles");
        }
    }

    internal override void StartBuff()
    {
        Game.VisualEffectController.SpawnSpriteEffect(Owner, Owner.Vector, "heart_t", 1f).Fades();
        Game.VisualEffectController.SpawnLightEffect(Owner, Owner.Vector, Color.white, 2, 1, 1).Fades();

        Owner.ManaPool.BurnMana(ManaColor.White, 3);

        Owner.Log($"{Owner.Name} channels some magic to .");
    }
}