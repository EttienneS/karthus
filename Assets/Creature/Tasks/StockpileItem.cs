using System.Collections.Generic;

public class StockpileItem : ITask
{
    public StockpileItem(string itemType, Stockpile stockpile)
    {
        SubTasks = new Queue<ITask>();
        Stockpile = stockpile;
        SubTasks.Enqueue(new MoveItemToCell(itemType, stockpile.Cell, false));
    }

    public Creature Creature { get; set; }
    public Stockpile Stockpile { get; set; }
    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.Data.CarriedItem == null)
            {
                return true;
            }

            Stockpile.AddItem(ItemController.Instance.ItemDataLookup[Creature.Data.CarriedItem]);
            Creature.Data.CarriedItem = null;
            return true;
        }

        return false;
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}