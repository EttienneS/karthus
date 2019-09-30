public class Suck : SpellBase
{
    public Suck()
    {
    }

    public override bool DoSpell()
    {
        Pipe toPipe;
        ManaPool fromPool;

        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);
        if (fromCell?.Structure?.IsBluePrint == false)
        {
            fromPool = fromCell.Structure.ManaPool;
        }
        else
        {
            return false;
        }

        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell?.Structure?.IsPipe() == true)
        {
            if (toCell.Structure is Pipe linkedPipe)
            {
                toPipe = linkedPipe;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }


        toPipe.Attunement = fromPool.GetManaWithMost();
        fromPool.Transfer(toPipe.ManaPool, toPipe.Attunement.Value, 1);

        return true;
    }
}