
using System;

public class Blow : SpellBase
{
    public Blow()
    {
    }

    public override bool DoSpell()
    {
        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);
        if (fromCell != null && fromCell.Structure != null && fromCell.Structure.IsPipe())
        {
            var linkedPipe = fromCell.Structure;

            if (linkedPipe.Properties[PipeConstants.Content] != PipeConstants.Nothing)
            {
                linkedPipe.ValueProperties[PipeConstants.Pressure]--;
                if (linkedPipe.ValueProperties[PipeConstants.Pressure] <= 0)
                {
                    linkedPipe.Properties[PipeConstants.Content] = PipeConstants.Nothing;
                }
                linkedPipe.Cell.UpdateTile();
                AssignedEntity.ManaPool.GainMana((ManaColor)Enum.Parse(typeof(ManaColor), linkedPipe.Properties[PipeConstants.Content]), 1);
            }
        }

        //var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        //if (toCell != null && toCell.Structure != null)
        //{
        //    var mana = toCell.Structure.ManaPool.GetManaWithMost();
        //    AssignedEntity.ManaPool.GainMana(mana, 1);
        //    toCell.Structure.ManaPool.BurnMana(mana, 1);
        //}



        return true;
    }
}