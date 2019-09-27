using System.Linq;

public class Suck : SpellBase
{

    public Suck()
    {
    }

    public override bool DoSpell()
    {
        var structures = new[]
        {
            AssignedEntity.Cell.GetNeighbor(Direction.N),
            AssignedEntity.Cell.GetNeighbor(Direction.E),
            AssignedEntity.Cell.GetNeighbor(Direction.S),
            AssignedEntity.Cell.GetNeighbor(Direction.W)
        };

        var suckedStructures = structures.Where(c => c?.Bound == true && c.Structure?.IsType(PipeConstants.Suckable) == true).ToList();

        if (suckedStructures.Count > 0)
        {
            var suckedStructure = suckedStructures.GetRandomItem().Structure;
            var mana = suckedStructure.ManaPool.GetRandomManaColorFromPool();
            AssignedEntity.ManaPool.GainMana(mana, 1);
            suckedStructure.ManaPool.BurnMana(mana, 1);
        }

        foreach (var linkedpipe in AssignedEntity.Cell.LinkedPipes)
        {
            var mana = AssignedEntity.ManaPool.GetRandomManaColorFromPool();
            if (linkedpipe.Properties[PipeConstants.Content] == PipeConstants.Nothing
                || linkedpipe.Properties[PipeConstants.Content] == mana.ToString())
            {
                linkedpipe.Properties[PipeConstants.Content] = mana.ToString();
                
                linkedpipe.ValueProperties[PipeConstants.Pressure]++;
                linkedpipe.Cell.UpdateTile();
                AssignedEntity.ManaPool.BurnMana(mana, 1);
                break;
            }
        }

        return true;
    }
}