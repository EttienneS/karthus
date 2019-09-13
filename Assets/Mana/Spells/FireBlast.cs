using System.Collections.Generic;
using System.Linq;

public class FireBlast : IAttack
{
    public int Range { get; set; } = 5;
    public IEntity Attacker { get; set; }
    public IEntity Target { get; set; }

    public bool Ready()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }
        if (!Attacker.ManaPool.HasMana(ManaColor.Red, 1))
        {
            Attacker.Task.AddSubTask(new Acrue(new Dictionary<ManaColor, int> { { ManaColor.Red, 1 } }));
            return false;
        }

        if (Attacker.Cell.DistanceTo(Target.Cell) > Range)
        {
            Attacker.Task.AddSubTask(new Move(Game.Map.GetCircle(Target.Cell, Range - 1).First()));
            return false;
        }
        return true;
    }

    public void Resolve()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }
        Attacker.Task.DoneEmote = "BLAST!";
        Attacker.ManaPool.BurnMana(ManaColor.Red, 1);

        Game.EffectController.SpawnEffect(Target.Cell, 0.5f);
        Game.LeyLineController.MakeChannellingLine(Attacker,
                                                   Target, 5, 0.5f,
                                                   ManaColor.Red);
        Target.Damage(5, ManaColor.Red);
    }
}