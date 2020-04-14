using UnityEngine;

public class Triage : BuffBase
{
    public Triage(string name) : base(name, cooldown: 1f, duration: 0.5f)
    {
    }

    public override int EstimateBuffEffect()
    {
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
        Game.Instance.VisualEffectController.SpawnLightEffect(Owner, Owner.Vector, ColorConstants.WhiteBase, 2, 1, 1).Fades();

        Owner.Log($"{Owner.Name} channels some magic to .");
    }
}