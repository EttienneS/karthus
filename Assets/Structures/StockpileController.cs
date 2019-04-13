using System.Collections.Generic;
using UnityEngine;

public class StockpileController : MonoBehaviour
{
    public Dictionary<int, Stockpile> StockpileLookup = new Dictionary<int, Stockpile>();

    public Stockpile StockpilePrefab;

    

    public Stockpile LoadStockpile(StockpileData data)
    {
        var stockpile = Instantiate(StockpilePrefab, transform);
        stockpile.Data = data;

        StockpileLookup.Add(stockpile.Data.Id, stockpile);
        return stockpile;
    }

    public Stockpile AddStockpile(string itemCategory)
    {
        var stockpile = Instantiate(StockpilePrefab, transform);
        stockpile.Data.ItemCategory = itemCategory;
        stockpile.Data.Id = StockpileLookup.Count + 1;
        StockpileLookup.Add(stockpile.Data.Id, stockpile);

        return stockpile;
    }

    internal Stockpile GetStockpile(int stockpileId)
    {
        return StockpileLookup[stockpileId];
    }

    internal void DestroyStockpile(StockpileData stockpile)
    {
        StockpileLookup.Remove(stockpile.Id);
    }
}