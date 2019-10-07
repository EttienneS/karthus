public class ManaBlast : EffectBase
{
    public override int Range { get { return 5; } }

    public override bool Ready()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        if (ManaCost == null)
        {
            ManaCost = ManaExtensions.GetCostPool((AssignedEntity.ManaPool.GetManaWithMost(), 1));
        }

        return true;
    }

    public override bool DoEffect()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        CreatureData.Task.DoneEmote = "Piss off ghost!!";

        Game.EffectController.SpawnEffect(Target.Cell, 0.5f);

        foreach (var kvp in ManaCost)
        {
            Game.LeyLineController.MakeChannellingLine(AssignedEntity, Target, 5, 0.5f, kvp.Key);
            Target.Damage(5, kvp.Key);
        }

        return true;
    }
}