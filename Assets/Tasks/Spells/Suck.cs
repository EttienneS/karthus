public class Suck : SpellBase
{
    public Suck()
    {
    }

    public override bool DoSpell()
    {
        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);
        if (fromCell?.Structure?.IsBluePrint == false)
        {
            var mana = fromCell.Structure.ManaPool.GetManaWithMost();
            AssignedEntity.ManaPool.GainMana(mana, 1);
            fromCell.Structure.ManaPool.BurnMana(mana, 1);
        }

        
        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell?.Structure?.IsPipe() == true)
        {
            var linkedPipe = toCell.Structure as Pipe;
            var mana = AssignedEntity.ManaPool.GetManaWithMost();

            if (AssignedEntity.ManaPool[mana].Total > 0
                && (!linkedPipe.Content.HasValue || linkedPipe.Content == mana))
            {
                AssignedEntity.ManaPool.BurnMana(mana, 1);

                linkedPipe.Content = mana;
                linkedPipe.Pressure++;
            }
        }

        return true;
    }
}