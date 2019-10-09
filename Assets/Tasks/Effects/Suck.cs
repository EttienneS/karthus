public class Suck : EffectBase
{
    public new int Range = -1;

    public Structure Structure;

    public Suck()
    {
    }

    public override bool DoEffect()
    {
        Pipe toPipe;
        ManaPool fromPool;

        if (Structure == null)
        {
            Structure = AssignedEntity as Structure;
        }

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

        if (!toPipe.Attunement.HasValue)
        {
            toPipe.Attunement = fromPool.GetManaWithMost();
        }

        if (fromPool.HasMana(toPipe.Attunement.Value))
        {
            fromPool.Transfer(toPipe.ManaPool, toPipe.Attunement.Value, 1);
        }

        return true;
    }
}