using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item itemPrefab;

    internal Dictionary<ItemData, Item> ItemDataLookup = new Dictionary<ItemData, Item>();
    internal Dictionary<int, Item> ItemIdLookup = new Dictionary<int, Item>();
    internal Dictionary<string, List<Item>> ItemCategoryIndex = new Dictionary<string, List<Item>>();
    internal Dictionary<string, List<Item>> ItemNameIndex = new Dictionary<string, List<Item>>();
    private static ItemController _instance;

    private Dictionary<string, Item> _allItemNames;

    public static ItemController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find(ControllerConstants.ItemController).GetComponent<ItemController>();
            }

            return _instance;
        }
    }

    internal Dictionary<string, Item> AllItemNames
    {
        get
        {
            if (_allItemNames == null)
            {
                _allItemNames = new Dictionary<string, Item>();
                foreach (var itemFile in FileController.Instance.ItemJson)
                {
                    var item = Instantiate(itemPrefab, transform);

                    item.Load(itemFile.text);
                    item.name = item.Data.Name;

                    AllItemNames.Add(item.Data.Name, item);
                }
            }
            return _allItemNames;
        }
    }

    internal void DestoyItem(ItemData data)
    {
        DestroyItem(ItemDataLookup[data]);
    }

    internal void DestroyItem(ItemData item)
    {
        if (ItemDataLookup.ContainsKey(item))
            DestroyItem(ItemDataLookup[item]);
    }

    internal void DestroyItem(int itemId)
    {
        DestroyItem(ItemIdLookup[itemId]);
    }

    internal void DestroyItem(Item item)
    {
        if (item.Cell != null)
        {
            item.Cell.ContainedItems.Remove(item.Data);
        }

        ItemCategoryIndex[item.Data.Category].Remove(item);
        ItemNameIndex[item.Data.Name].Remove(item);
        ItemIdLookup.Remove(item.Data.Id);
        ItemDataLookup.Remove(item.Data);

        Destroy(item.gameObject);
    }

    internal ItemData FindClosestItemByName(CellData centerPoint, string type, bool allowStockpiled)
    {
        return FindClosestItem(ItemNameIndex, centerPoint, type, allowStockpiled);
    }

    internal ItemData FindClosestItemOfType(CellData centerPoint, string type, bool allowStockpiled)
    {
        return FindClosestItem(ItemCategoryIndex, centerPoint, type, allowStockpiled);
    }

    internal ItemData FindClosestItem(Dictionary<string, List<Item>> lookup, CellData centerPoint, string type, bool allowStockpiled)
    {
        if (!lookup.ContainsKey(type) || lookup[type].Count == 0)
        {
            // no registred item of the given type exists
            return null;
        }
        else
        {
            // registered items found
            var checkedCells = new HashSet<CellData>();
            var closest = int.MaxValue;
            ItemData closestItem = null;
            foreach (var item in lookup[type])
            {
                if (item.Data.Reserved || (!allowStockpiled && item.Data.StockpileId > 0))
                {
                    continue;
                }
                if (checkedCells.Add(item.Cell))
                {
                    var distance = centerPoint.Coordinates.DistanceTo(item.Cell.Coordinates);
                    if (distance < closest)
                    {
                        closest = distance;
                        closestItem = item.Data;
                    }
                }
            }

            return closestItem;
        }
    }

    private int IdCounter = 0;

    public Item GetItem(ItemData data)
    {
        var item = Instantiate(itemPrefab, transform);
        item.Data = data;
        item.Data.Id = IdCounter++;

        IndexItem(item);
        return item;
    }

    internal Item GetItem(string name)
    {
        var item = Instantiate(AllItemNames[name], transform);
        item.Load(FileController.Instance.ItemLookup[name].text);

        item.Data.Id = IdCounter++;
        IndexItem(item);

        return item;
    }

    internal Item LoadItem(ItemData savedItem)
    {
        var item = Instantiate(AllItemNames[savedItem.Name], transform);
        item.Data = savedItem;
        IndexItem(item);

        return item;
    }

    private void IndexItem(Item item)
    {
        if (!ItemCategoryIndex.ContainsKey(item.Data.Category))
        {
            ItemCategoryIndex.Add(item.Data.Category, new List<Item>());
        }
        ItemCategoryIndex[item.Data.Category].Add(item);

        if (!ItemNameIndex.ContainsKey(item.Data.Name))
        {
            ItemNameIndex.Add(item.Data.Name, new List<Item>());
        }
        ItemNameIndex[item.Data.Name].Add(item);
        ItemDataLookup.Add(item.Data, item);
        ItemIdLookup.Add(item.Data.Id, item);

        item.name = $"{item.Data.Category} ({item.Data.Id})";
    }
}