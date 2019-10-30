using System.Linq;

public class Bite : EffectBase
{
    public Bite()
    {
        Cost = ManaExtensions.GetCostPool((ManaColor.Red, 1));
    }

    public override bool DoEffect()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        Target.Damage(AssignedEntity, TargetType.Biggest, 5, 0.5f);

        if (AssignedEntity is CreatureData creature)
            creature.Task.AddSubTask(new Move(Target.Cell.Neighbors.Where(n => n != null && n.TravelCost > 0).ToList().GetRandomItem()));

        return true;
    }
}