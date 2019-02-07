using System.Collections.Generic;

public class DropHeldItem : ITask
{
    public Creature Creature { get; set; }

    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        return Creature.Data.CarriedItem == null;
    }

    public void Update()
    {
        var item =ItemController.Instance.ItemDataLookup[ Creature.Data.CarriedItem];
        Creature.Data.CarriedItem = null;

        item.SpriteRenderer.sortingLayerName = "Item";
        Creature.Data.CurrentCell.LinkedGameObject.AddContent(item.gameObject, true);
    }
}