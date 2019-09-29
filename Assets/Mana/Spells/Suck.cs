
public class Suck : SpellBase
{
    public Suck()
    {
    }

    public override bool DoSpell()
    {
        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);
        if (fromCell != null && fromCell.Structure != null)
        {
            var mana = fromCell.Structure.ManaPool.GetManaWithMost();
            AssignedEntity.ManaPool.GainMana(mana, 1);
            fromCell.Structure.ManaPool.BurnMana(mana, 1);
        }

        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell != null && toCell.Structure != null && toCell.Structure.IsPipe())
        {
            var linkedPipe = toCell.Structure;

            var mana = AssignedEntity.ManaPool.GetManaWithMost();
            if (linkedPipe.Properties[PipeConstants.Content] == PipeConstants.Nothing
                || linkedPipe.Properties[PipeConstants.Content] == mana.ToString())
            {
                linkedPipe.Properties[PipeConstants.Content] = mana.ToString();

                linkedPipe.ValueProperties[PipeConstants.Pressure]++;
                linkedPipe.Cell.UpdateTile();
                AssignedEntity.ManaPool.BurnMana(mana, 1);
            }
        }

        return true;
    }
}