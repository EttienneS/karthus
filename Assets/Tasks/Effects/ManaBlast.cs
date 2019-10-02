public class ManaBlast : EffectBase
{
    public int Range { get; set; } = 5;

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

        if (AssignedEntity.Cell.DistanceTo(Target.Cell) > Range)
        {
            var spot = Game.Map.GetCircle(Target.Cell, Range - 1);
            spot.Shuffle();
            AssignedEntity.Task.AddSubTask(new Move(spot[0]));
            return false;
        }
        return true;
    }

    public override bool DoEffect()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        AssignedEntity.Task.DoneEmote = "Piss off ghost!!";

        Game.EffectController.SpawnEffect(Target.Cell, 0.5f);

        foreach (var kvp in ManaCost)
        {
            Game.LeyLineController.MakeChannellingLine(AssignedEntity, Target, 5, 0.5f, kvp.Key);
            Target.Damage(5, kvp.Key);
        }

        return true;
    }
}