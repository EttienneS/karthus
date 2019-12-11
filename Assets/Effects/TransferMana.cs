public class TransferMana : EffectBase
{
    public TransferMana()
    {
    }

    public int Amount { get; set; }
    public ManaColor[] Colors { get; set; }

    public override bool DoEffect()
    {
        foreach (var color in Colors)
        {
            AssignedEntity.ManaValue.Transfer(Target.ManaValue, color, Amount);
        }
        return true;
    }
}