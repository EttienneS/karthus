 using System.Linq;
using UnityEngine;

public class Siphon : EffectBase
{
    public int Size;
    public new int Range = 1;

    public Siphon()
    {
    }

    public Siphon(int size)
    {
        Size = size;
    }

    public override bool DoEffect()
    {
        ManaColor col;

        var siphonedCell = Game.Map.GetCircle(AssignedEntity.Cell, Size)
                                   .Where(c => c != AssignedEntity.Cell && !c.HasBuilding && c.Bound)
                                   .GetRandomItem();

        switch (siphonedCell.CellType)
        {
            case CellType.Forest:
            case CellType.Grass:
                col = ManaColor.Green;
                break;

            case CellType.Water:
                col = ManaColor.Blue;
                break;

            case CellType.Dirt:
            case CellType.Stone:
            case CellType.Mountain:
                col = ManaColor.Red;
                break;

            default:
                return false;
        }

        var amount = Random.Range(1, 3);
        AssignedEntity.ManaPool.GainMana(col, amount);
        Game.VisualEffectController.SpawnLightEffect(AssignedEntity.Cell, col.GetActualColor(), 1.5f, amount * 2f, 3)
                                      .Fades();

        Game.VisualEffectController.SpawnLightEffect(siphonedCell, Color.magenta, 1.5f, 8, 8)
                                      .Fades();
        Game.Map.Unbind(siphonedCell);
        return true;
    }
}