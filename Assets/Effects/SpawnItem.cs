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
            Game.ItemController.SpawnItem(ItemName, AssignedEntity.Cell);
        }
        return true;
    }
}