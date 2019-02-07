using System.Collections.Generic;
using UnityEngine;

public class StockpileController : MonoBehaviour
{
    public Dictionary<string, Stockpile> StockpileLookup = new Dictionary<string, Stockpile>();

    public Stockpile StockpilePrefab;

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