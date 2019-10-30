public class ManaBlast : EffectBase
{
    public new int Range = 5;

    public override bool Ready()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        if (Cost == null)
        {
            Cost = ManaExtensions.GetCostPool((AssignedEntity.ManaPool.GetManaWithMost(), 1));
        }

        return true;
    }

    public override bool DoEffect()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        Game.VisualEffectController.SpawnEffect(AssignedEntity, Target.Cell, 0.5f);

        foreach (var kvp in Cost)
        {
            Game.LeyLineController.MakeChannellingLine(AssignedEntity, Target, 5, 0.5f, kvp.Key);
            Target.Damage(AssignedEntity, TargetType.Biggest, 5, 0.5f);
        }

        return true;
    }
}