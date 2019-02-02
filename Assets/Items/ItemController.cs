using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item itemPrefab;

    private static ItemController _instance;

    internal Dictionary<string, Item> AllItemTypes = new Dictionary<string, Item>();

    internal Dictionary<string, List<Item>> ItemTypeIndex = new Dictionary<string, List<Item>>();

    public static ItemController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("ItemController").GetComponent<ItemController>();
            }

            return _instance;
        }
    }

    public void Start()
    {
        foreach (var itemFile in FileController.Instance.ItemJson)
        {
            var item = Instantiate(itemPrefab, transform);

            item.Load(itemFile.text);
            item.name = item.Data.Name;

            AllItemTypes.Add(item.Data.Name, item);
        }
    }

    internal Item GetItem(string name)
    {
        var item = Instantiate(AllItemTypes[name], transform);

        if (!ItemTypeIndex.ContainsKey(item.Data.ItemType))
        {
            ItemTypeIndex.Add(item.Data.ItemType, new List<Item>());
        }

        ItemTypeIndex[item.Data.ItemType].Add(item);
        return item;
    }

    internal Item FindClosestItemOfType(Cell centerPoint, string type, bool allowStockpiled)
    {
        if (allowStockpiled)
        {
            var stockpiledItem = StockpileController.Instance.FindClosestItemInStockpile(type, centerPoint);

            if (stockpiledItem != null)
            {
                return stockpiledItem;
            }
        }

        var searchRadius = 3;
        var maxRadius = MapGrid.Instance.Map.GetLength(0);
        var checkedCells = new HashSet<Cell>();
        do
        {
            var searchArea = MapGrid.Instance.GetCircle(centerPoint, searchRadius);
            searchArea.Reverse();
            foreach (var cell in searchArea)
            {
                if (checkedCells.Add(cell))
                {
                    foreach (var item in cell.ContainedItems.Where(i => i.Data.ItemType == type && !i.Data.Reserved))
                    {
                        if (!string.IsNullOrEmpty(item.Data.StockpileId))
                        {
                            continue;
                        }

                        return item;
                    }
                }
            }

            searchRadius += 3;
        }
        while (searchRadius <= maxRadius);

        return null;
    }

    internal void DestoyItem(Item item)
    {
        if (item.Cell != null)
        {
            item.Cell.ContainedItems.Remove(item);
        }

        ItemTypeIndex[item.Data.ItemType].Remove(item);
        Destroy(item.gameObject);
    }
}