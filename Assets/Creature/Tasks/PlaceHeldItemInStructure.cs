public class PlaceHeldItemInStructure : TaskBase
{
    private StructureData _structure { get; set; }

    public PlaceHeldItemInStructure(StructureData structure)
    {
        _structure = structure;
    }

    public override bool Done()
    {
        return Creature.CarriedItem == null;
    }

    public override void Update()
    {
        var item = ItemController.Instance.ItemDataLookup[Creature.CarriedItem];
        Creature.CarriedItem = null;

        Creature.CurrentCell.LinkedGameObject.AddContent(item.gameObject, true);
        _structure.AddItem(item.Data);
        item.SpriteRenderer.sortingLayerName = "Item";
    }
}