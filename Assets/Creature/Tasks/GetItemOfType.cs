using System.Collections.Generic;
using UnityEngine;

public class GetItemOfType : ITask
{
    private Cell _itemLocation;
    public Creature Creature { get; set; }
    public bool AllowStockpiled { get; set; }

    public GetItemOfType(string itemType, bool allowStockpiled)
    {
        AllowStockpiled = allowStockpiled;
        ItemType = itemType;
        SubTasks = new Queue<ITask>();
    }

    private Item _item;

    public string ItemType { get; set; }
    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        if (_item != null && Taskmaster.QueueComplete(SubTasks))
        {
            if (!string.IsNullOrEmpty(_item.Data.StockpileId))
            {
                var pile = StockpileController.Instance.GetStockpile(_item.Data.StockpileId);
                _item = pile.GetItem(_item);
            }

            Creature.CarriedItem = _item;
            _item.SpriteRenderer.color = Color.white;
            return true;
        }

        return false;
    }

    public void Update()
    {
        if (_item == null)
        {
            _item = MapGrid.Instance.FindClosestItemOfType(Creature.CurrentCell, ItemType, AllowStockpiled);
            _item.Data.Reserved = true;
            UpdateTargetItem();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }

    private void UpdateTargetItem()
    {
        SubTasks = new Queue<ITask>();
        _itemLocation = MapGrid.Instance.GetCellAtPoint(_item.transform.position);

        var moveTask = new Move(_itemLocation)
        {
            Creature = Creature
        };
        SubTasks.Enqueue(moveTask);
        _item.SpriteRenderer.color = Color.magenta;
    }
}