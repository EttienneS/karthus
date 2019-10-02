using System.Collections.Generic;
using System.Linq;

public class Bite : EffectBase
{
    public int Range { get; set; } = 0;


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
        AssignedEntity.Task.DoneEmote = "OMNOMONOM";
        Target.Damage(2, ManaColor.Black);
        AssignedEntity.Task.AddSubTask(new Move(Target.Cell.Neighbors.Where(n => n != null && n.TravelCost > 0).ToList().GetRandomItem()));

        return true;
    }

    public override bool Ready()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }
        if (AssignedEntity.Cell.DistanceTo(Target.Cell) > Range)
        {
            AssignedEntity.Task.AddSubTask(new Move(Target.Cell));
            return false;
        }
        return true;
    }
}