public class Blow : EffectBase
{
    public new int Range = -1;

    public Structure Structure;

    public Blow()
    {
    }

    public override bool DoEffect()
    {
        Pipe fromPipe;
        ManaPool toPool;

        if (Structure == null)
        {
            Structure = AssignedEntity as Structure;
        }

        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);

        if (fromCell?.Structure?.IsPipe() == true)
        {
            var linkedPipe = fromCell.Structure as Pipe;

            if (linkedPipe.Attunement.HasValue)
            {
                fromPipe = linkedPipe;
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

        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell?.Structure?.IsBluePrint == false)
        {
            toPool = toCell.Structure.ManaPool;
        }
        else
        {
            return false;
        }

        if (fromPipe.ManaPool.HasMana(fromPipe.Attunement.Value))
        {
            fromPipe.ManaPool.Transfer(toPool, fromPipe.Attunement.Value, 1);
        }
        else
        {
            fromPipe.Attunement = null;
            fromPipe.Cell.UpdateTile();
        }

        return true;
    }
}