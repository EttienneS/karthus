using System.Collections.Generic;

public class StockpileItem : ITask
{
    public Queue<ITask> SubTasks { get; set; }
    public Creature Creature { get; set; }
    public string TaskId { get; set; }
    public Stockpile Stockpile { get; set; }

    public StockpileItem(string itemType, Stockpile stockpile)
    {
        SubTasks = new Queue<ITask>();
        Stockpile = stockpile;
        SubTasks.Enqueue(new MoveItemToCell(itemType, stockpile.Cell, false));
    }


    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.CarriedItem == null)
            {
                return true;
            }

            Stockpile.AddItem(Creature.CarriedItem);
            Creature.CarriedItem = null;
            return true;
        }

        return false;
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}