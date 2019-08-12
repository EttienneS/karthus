using System.Collections.Generic;
using System.Linq;

public class FireBlast : IAttack
{
    public int Range { get; set; } = 3;
    public IEntity Attacker { get; set; }
    public IEntity Target { get; set; }

    public bool Ready()
    {
        if (!Attacker.ManaPool.HasMana(ManaColor.Red, 1))
        {
            Attacker.Task.AddSubTask(new Acrue(new Dictionary<ManaColor, int> { { ManaColor.Red, 1 } }));
            return false;
        }

        if (Pathfinder.Distance(Game.MapGrid.GetCellAtCoordinate(Attacker.Coordinates),
            Game.MapGrid.GetCellAtCoordinate(Target.Coordinates), Mobility.Walk) <= Range)
        {
            Attacker.Task.AddSubTask(new Move(Game.MapGrid.GetCircle(Target.Coordinates, Range - 1).First().Coordinates));
            return false;
        }
        return true;
    }

    public void Resolve()
    {
        Attacker.ManaPool.BurnMana(ManaColor.Red, 1);

        Game.EffectController.SpawnEffect(Target.Coordinates, 3);
        Game.LeyLineController.MakeChannellingLine(Attacker.Coordinates.ToTopOfMapVector(),
                                                   Target.Coordinates.ToTopOfMapVector(), 5, 3,
                                                   ManaColor.Red);
        Target.Damage(5, ManaColor.Red);
    }
}