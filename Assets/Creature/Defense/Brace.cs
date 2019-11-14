public class Brace : DefensiveActionBase
{
    public Brace(string name) : base(name)
    {
        ActivationTIme = 0.3f;
    }

    public override int Defend(string attackName, int incomingDamage, DamageType damageType)
    {
        Game.VisualEffectController.SpawnSpriteEffect(Owner, Owner.Vector, "armor_t", 1f).Fades();

        Limb.Owner.Log($"{Limb.Owner.Name} braces their {Limb.Name} against the {attackName}");
        return (int)(incomingDamage * 0.75);
    }

    public override int EstimateDefense(OffensiveActionBase offensiveActionBase)
    {
        return (int)(offensiveActionBase.PredictDamage(Owner) * 0.75);
    }
}