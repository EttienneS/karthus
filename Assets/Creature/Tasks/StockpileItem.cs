﻿using Newtonsoft.Json;

public class StockpileItem : TaskBase
{
    public StockpileItem()
    {
    }

    public StockpileItem(string itemCategory, int stockpileId)
    {
        StockpileId = stockpileId;

        AddSubTask(new MoveItemToCell(itemCategory, Stockpile.Data.Coordinates, false, false, GetItem.SearchBy.Category));
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