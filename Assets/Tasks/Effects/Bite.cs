using System.Collections.Generic;
using System.Linq;

public class Bite : EffectBase
{


    public Bite()
    {
        ManaCost = ManaExtensions.GetCostPool((ManaColor.Red, 1));
    }

    public override bool DoEffect()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }

        CreatureData.Task.DoneEmote = "OMNOMONOM";
        Target.Damage(2, ManaColor.Black);
        CreatureData.Task.AddSubTask(new Move(Target.Cell.Neighbors.Where(n => n != null && n.TravelCost > 0).ToList().GetRandomItem()));

        return true;
    }


}