using System;

public class Blow : SpellBase
{
    public Blow()
    {
    }

    public override bool DoSpell()
    {
        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);
        if (fromCell?.Structure?.IsPipe() == true)
        {
            var linkedPipe = fromCell.Structure as Pipe;

            if (linkedPipe.Content.HasValue)
            {
                AssignedEntity.ManaPool.GainMana(linkedPipe.Content.Value, 1);
                linkedPipe.Pressure--;
            }
        }

        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell?.Structure?.IsBluePrint == false)
        {
            var mana = AssignedEntity.ManaPool.GetManaWithMost();
            if (AssignedEntity.ManaPool[mana].Total > 0)
            {
                AssignedEntity.ManaPool.BurnMana(mana, 1);
                toCell.Structure.ManaPool.GainMana(mana, 1);
            }
        }

        return true;
    }
}