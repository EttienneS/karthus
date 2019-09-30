using System.Linq;
using UnityEngine;

public class Siphon : SpellBase
{
    public int Size;

    public Siphon()
    {
    }

    public Siphon(int size)
    {
        Size = size;
    }

    public override bool DoSpell()
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

        AssignedEntity.ManaPool.GainMana(col, Random.Range(1, 3));
        //Game.Map.Unbind(siphonedCell);
        return true;
    }
}