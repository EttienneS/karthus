using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

[JsonConverter(typeof(StringEnumConverter))]
public enum DamageType
{
    Slashing, Bludgeoning, Piercing, Energy
}

public abstract class OffensiveActionBase
{
    public DamageType DamageType { get; set; }

    protected OffensiveActionBase(string name, DamageType damageType, float activationTime, int range)
    {
        Name = name;
        DefensiveActions = new List<DefensiveActionBase>();
        DamageType = damageType;
        ActivationTime = activationTime;
        Range = range;
    }

    [JsonIgnore]
    public abstract int Damage { get; }
    public int Range { get; }

    [JsonIgnore]
    public Limb Limb { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public List<DefensiveActionBase> DefensiveActions { get; set; }

    [JsonIgnore]
    public Limb TargetLimb { get; set; }

    public float ActivationTime { get; set; }

    public float Progress { get; set; }

    [JsonIgnore]
    public Creature Owner
    {
        get
        {
            return Limb.Owner;
        }
    }

    public int PredictDamage(Creature creature)
    {
        var iterations = 10;
        var total = 0;
        for (int i = 0; i < iterations; i++)
        {
            total += TargetLimb.GetDamageAfterResistance(DamageType, Damage);
        }

        return (int)(total / iterations / ActivationTime);
    }

    public void Reset()
    {
        Progress = 0;
        Limb.Busy = false;

        foreach (var def in DefensiveActions)
        {
            def.Limb.Busy = false;
        }

        DefensiveActions.Clear();
    }

    internal bool Done(float delta)
    {
        Progress += delta;
        return Progress >= ActivationTime;
    }

    internal virtual void Resolve()
    {
        var damage = Damage;
        foreach (var def in DefensiveActions)
        {
            damage = def.Defend(Name, damage, DamageType);
        }

        if (damage > 0)
        {
            Owner.Log($"{Owner.Name}'s {Name} hits {TargetLimb.Owner.Name}'s {TargetLimb.Name}");
            TargetLimb.Damage(DamageType, damage);
        }
        else
        {
            Owner.Log($"{Owner.Name}'s {Name} hits {TargetLimb.Owner.Name}'s {TargetLimb.Name} but does no damage!");
        }
    }

    internal float TimeToComplete()
    {
        return ActivationTime - Progress;
    }
}