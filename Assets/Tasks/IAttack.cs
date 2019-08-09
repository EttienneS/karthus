using System.Collections.Generic;

public interface IAttack
{
    IEntity Attacker { get; set; }

    bool Ready();

    bool Resolve(IEntity target);
}

public class FireBlast : IAttack
{
    public IEntity Attacker { get; set; }

    public bool Ready()
    {
        if (!Attacker.ManaPool.HasMana(ManaColor.Red, 1))
        {
            Attacker.Task.AddSubTask(new Acrue(new Dictionary<ManaColor, int> { { ManaColor.Red, 1 } }));
            return false;
        }
        return true;
    }

    public bool Resolve(IEntity target)
    {
        var cell = Game.MapGrid.GetCellAtCoordinate(target.Coordinates);
        cell.CellType = CellType.Mountain;

        Game.MapGrid.RefreshCell(cell);
        Game.EffectController.SpawnEffect(target.Coordinates, 2f);
        //Game.CreatureController.DestroyCreature(IdService.CreatureLookup[target].CreatureRenderer);
        return true;
    }
}