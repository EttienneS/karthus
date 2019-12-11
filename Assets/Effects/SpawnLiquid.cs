public class SpawnLiquid : EffectBase
{
    public ManaColor Color;

    public float Amount;

    public override bool DoEffect()
    {
        AssignedEntity.Cell.AddLiquid(Color, Amount);
        return true;
    }
}