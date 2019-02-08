using System;
using System.Collections.Generic;
using UnityEngine;

public class StockpileController : MonoBehaviour
{
    public Dictionary<int, Stockpile> StockpileLookup = new Dictionary<int, Stockpile>();

    public Stockpile StockpilePrefab;

    private static StockpileController _instance;

    public static StockpileController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("StockpileController").GetComponent<StockpileController>();
            }

            return _instance;
        }
    }

    public Stockpile LoadStockpile(StockpileData data)
    {
        var stockpile = Instantiate(StockpilePrefab, transform);
        stockpile.Data = data;

        StockpileLookup.Add(stockpile.Data.Id, stockpile);
        return stockpile;
    }


    public Stockpile AddStockpile(string itemTypeName)
    {
        var stockpile = Instantiate(StockpilePrefab, transform);
        stockpile.Data.ItemType = itemTypeName;
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