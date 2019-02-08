using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class StockpileItem : TaskBase
{
    public StockpileItem(string itemType, int stockpileId)
    {
        SubTasks = new Queue<TaskBase>();
        StockpileId = stockpileId;
        
        SubTasks.Enqueue(new MoveItemToCell(itemType, Stockpile.Data.Coordinates, false));
    }

    public int StockpileId { get; set; }

    [JsonIgnore]
    private Stockpile Stockpile
    {
        get
        {
            return StockpileController.Instance.GetStockpile(StockpileId);
        }
    }

    public override bool Done()
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

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}