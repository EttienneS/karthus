
using System;

[Serializable]
public class DropHeldItem : TaskBase
{
    public override bool Done()
    {
        return Creature.CarriedItem == null;
    }

    public override void Update()
    {
        var item = ItemController.Instance.ItemDataLookup[Creature.CarriedItem];
        Creature.CarriedItem = null;

        item.SpriteRenderer.sortingLayerName = "Item";
        Creature.CurrentCell.LinkedGameObject.AddContent(item.gameObject, true);
    }
}