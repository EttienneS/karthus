using Newtonsoft.Json;
using System;

public abstract class DefensiveActionBase
{
    protected DefensiveActionBase(string name)
    {
        Name = name;
    }

    [JsonIgnore]
    public Limb Limb { get; set; }

    public string Name { get; set; }

    public float ActivationTIme { get; set; }

    public abstract int EstimateDefense(OffensiveActionBase offensiveActionBase);

    public abstract int Defend(string attackName, int incomingDamage, DamageType damageType);

    internal int PredictDamageAfterDefense(OffensiveActionBase offensiveActionBase)
    {
        var total = 0;

        for (int i = 0; i < 10; i++)
        {
            total += Math.Max(0, EstimateDefense(offensiveActionBase));
        }

        return total / 10;
    }

    [JsonIgnore]
    public Creature Owner
    {
        get
        {
            return Limb.Owner;
        }
    }
}