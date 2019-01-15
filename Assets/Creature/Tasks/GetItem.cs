using System.Collections.Generic;
using UnityEngine;

public class GetItem : ITask
{
    private Cell _itemLocation;
    public Creature Creature { get; set; }

    public GetItem(Item item)
    {
        Item = item;
        UpdateTargetItem();
    }

    public Item Item { get; set; }
    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            Creature.CarriedItem = Item;
            Item.SpriteRenderer.color = Color.white;
            return true;
        }

        return false;
    }


    public void Update()
    {
        if (Item == null)
        {
            Item = MapGrid.Instance.FindClosestItem(_itemLocation);
            Item.Reserved = true;
            UpdateTargetItem();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }

    private void UpdateTargetItem()
    {
        SubTasks = new Queue<ITask>();
        _itemLocation = MapGrid.Instance.GetCellAtPoint(Item.transform.position);

        var moveTask = new Move(_itemLocation)
        {
            Creature = Creature
        };
        SubTasks.Enqueue(moveTask);
        Item.SpriteRenderer.color = Color.magenta;
    }
}