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
            AssignedEntity.ManaPool.Transfer(Target.ManaPool, color, Amount);
        }
        return true;
    }
}