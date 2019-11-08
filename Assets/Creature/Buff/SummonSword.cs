public class SummonSword : BuffBase
{
    public SummonSword(string name) : base(name, cooldown: 2f, duration: 10f)
    {
    }

    public override int EstimateBuffEffect()
    {
        return (int)(15 * Owner.Aggression);
    }

    private Limb _sword;

    public Limb GetSword()
    {
        var limb = new Limb("Sword", 20, (DamageType.Bludgeoning, 3), (DamageType.Piercing, 3), (DamageType.Slashing, 3), (DamageType.Energy, 3));
        limb.AddDefensiveAction(new Block("sword block", 5, DamageType.Bludgeoning, DamageType.Slashing));
        limb.AddOffensiveAction(new Strike("sword slash", 10, DamageType.Energy));

        return limb;
    }

    internal override void StartBuff()
    {
        _sword = GetSword();
        Owner.AddLimb(_sword);

        Owner.Log($"{Owner.Name} summons a sword of pure force.");
    }

    internal override void EndBuff()
    {
        Owner.Log($"{Owner.Name}'s sword dissapates.");
        Owner.Limbs.Remove(_sword);
    }
}