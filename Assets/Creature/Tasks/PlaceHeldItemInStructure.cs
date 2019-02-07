public class PlaceHeldItemInStructure : TaskBase
{
    public PlaceHeldItemInStructure(StructureData structure)
    {
        Structure = structure;
    }

    public StructureData Structure { get; set; }

    public override bool Done()
    {
        return Creature.CarriedItem == null;
    }

    public override void Update()
    {
        var item = ItemController.Instance.ItemDataLookup[Creature.CarriedItem];
        Creature.CarriedItem = null;

        Creature.CurrentCell.LinkedGameObject.AddContent(item.gameObject, true);
        Structure.AddItem(item.Data);
        item.SpriteRenderer.sortingLayerName = "Item";
    }
}