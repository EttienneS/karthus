using System.Collections.Generic;
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

        
        Target.Damage(2, ManaColor.Black);

        if (AssignedEntity is CreatureData creature)
            creature.Task.AddSubTask(new Move(Target.Cell.Neighbors.Where(n => n != null && n.TravelCost > 0).ToList().GetRandomItem()));

        return true;
    }


}