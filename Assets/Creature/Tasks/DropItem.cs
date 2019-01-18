using System.Collections.Generic;

public class DropHeldItem : ITask
{
    public Creature Creature { get; set; }

    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        return Creature.CarriedItem == null;
    }

    public void Update()
    {
        var item = Creature.CarriedItem;
        Creature.CarriedItem = null;

        item.SpriteRenderer.sortingLayerName = "Item";
        Creature.CurrentCell.AddContent(item.gameObject, true);
    }
}
