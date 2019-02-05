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
        if (!ItemTypeIndex.ContainsKey(type) || ItemTypeIndex[type].Count == 0)
        {
            // no registred item of the given type exists
            return null;
        }
        else
        {
            // registered items found
            var checkedCells = new HashSet<Cell>();
            var closest = int.MaxValue;
            Item closestItem = null;
            foreach (var item in ItemTypeIndex[type])
            {
                if (item.Data.Reserved || (!allowStockpiled && !string.IsNullOrEmpty(item.Data.StockpileId)))
                {
                    continue;
                }
                if (checkedCells.Add(item.Cell))
                {
                    var distance = centerPoint.Coordinates.DistanceTo(item.Cell.Coordinates);
                    if (distance < closest)
                    {
                        closest = distance;
                        closestItem = item;
                    }
                }
            }

            return closestItem;
        }



        return null;
    }

    public Item FindItemInArea(Cell centerPoint, string type, bool allowStockpiled)
    {
        // scan for item in radius in ever growing circles
        // very slow but potentially useful
        var checkedCells = new HashSet<Cell>();
        var searchRadius = 3;
        var maxRadius = MapGrid.Instance.Map.GetLength(0);
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