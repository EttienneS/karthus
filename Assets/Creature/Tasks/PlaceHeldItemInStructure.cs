using System.Collections.Generic;

public class PlaceHeldItemInStructure : ITask
{
    public Creature Creature { get; set; }
    private Structure _structure { get; set; }

    public Queue<ITask> SubTasks { get; set; }

    public string TaskId { get; set; }

    public PlaceHeldItemInStructure(Structure structure)
    {
        _structure = structure;
    }

    public bool Done()
    {
        return Creature.Data.CarriedItem == null;
    }

    public void Update()
    {
        var item = ItemController.Instance.ItemDataLookup[Creature.Data.CarriedItem];
        Creature.Data.CarriedItem = null;

        Creature.Data.CurrentCell.LinkedGameObject.AddContent(item.gameObject, true);
        _structure.Data.AddItem(item);
        item.SpriteRenderer.sortingLayerName = "Item";
    }
}