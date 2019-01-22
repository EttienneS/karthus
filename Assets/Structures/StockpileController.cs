using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockpileController : MonoBehaviour
{
    public Dictionary<string, Stockpile> StockpileLookup = new Dictionary<string, Stockpile>();

    public Stockpile StockpilePrefab;

    public Item FindClosestItemInStockpile(string itemType, Cell cell)
    {
        var cheapestCost = float.MaxValue;
        Item closestItem = null;
        foreach (var stockpile in StockpileLookup.Values)
        {
            var item = stockpile.GetItemOfType(itemType);
            if (item != null)
            {
                var cost = Pathfinder.GetPathCost(Pathfinder.FindPath(cell, stockpile.Cell));
                if (cost < cheapestCost)
                {
                    cheapestCost = cost;
                    closestItem = item;
                }
            }
        }

        return closestItem;
    }

    internal Stockpile GetStockpile(string stockpileId)
    {
        return StockpileLookup[stockpileId];
    }

    public Stockpile AddStockpile(string itemTypeName)
    {
        var stockpile = Instantiate(StockpilePrefab, transform);
        stockpile.ItemType = itemTypeName;

        StockpileLookup.Add(stockpile.StockpileId, stockpile);

        return stockpile;
    }

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

}
