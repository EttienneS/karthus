using System.Linq;

public class SpawnItem : EffectBase
{
    public string ItemName;

    public override bool DoEffect()
    {
        var existing = AssignedEntity.Cell.Items.FirstOrDefault(i => i.Name == ItemName);

        if (existing != null)
        {
            existing.Amount++;
        }
        else
        {
            var item = Game.ItemController.SpawnItem(ItemName, AssignedEntity.Cell);
            // center item in spawner cell
            item.Coords = (AssignedEntity.Cell.Vector.x, AssignedEntity.Cell.Vector.y);
        }
        return true;
    }
}