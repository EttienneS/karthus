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
        return Creature.CarriedItem == null;
    }

    public void Update()
    {
        _structure.Data.AddItem(Creature.CarriedItem);
        Creature.CarriedItem.SpriteRenderer.sortingLayerName = "Item";
        Creature.CarriedItem = null;
    }
}