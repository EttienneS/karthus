using Newtonsoft.Json;
using System;
using System.Collections.Generic;


public class StockpileItem : TaskBase
{
    public StockpileItem()
    {
    }

    public StockpileItem(string itemType, int stockpileId)
    {
        StockpileId = stockpileId;

        AddSubTask(new MoveItemToCell(itemType, Stockpile.Data.Coordinates, false, false));
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
            Creature.DropItem();

            Creature.UpdateMemory(Context, MemoryType.Stockpile, Stockpile.Data.GetGameId());
            return true;
        }

        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}