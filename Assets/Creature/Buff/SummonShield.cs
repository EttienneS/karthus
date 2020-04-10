public class SummonShield : BuffBase
{
    public SummonShield(string name) : base(name, cooldown: 2f, duration: 10f)
    {
    }

    public override int EstimateBuffEffect()
    {
        return (int)(10 / Owner.Aggression);
    }

    private Limb _shieldLimb;

    public Limb GetShieldLimb()
    {
        var limb = new Limb("Shield", 25, (DamageType.Bludgeoning, 10), (DamageType.Piercing, 10), (DamageType.Slashing, 10), (DamageType.Energy, 10));
        limb.AddDefensiveAction(new Block("shield block", 10, DamageType.Bludgeoning, DamageType.Piercing, DamageType.Slashing, DamageType.Energy));
        limb.AddOffensiveAction(new Strike("shield bash", 6, DamageType.Bludgeoning));

        return limb;
    }

    internal override void StartBuff()
    {
        _shieldLimb = GetShieldLimb();
        Owner.AddLimb(_shieldLimb);

        Owner.Log($"{Owner.Name} summons a shield of pure force.");
    }

    internal override void EndBuff()
    {
        Owner.Log($"{Owner.Name}'s shield dissapates.");
        Owner.Limbs.Remove(_shieldLimb);
    }
}