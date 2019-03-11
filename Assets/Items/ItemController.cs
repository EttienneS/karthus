using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item itemPrefab;

    internal Dictionary<ItemData, Item> ItemDataLookup = new Dictionary<ItemData, Item>();
    internal Dictionary<int, Item> ItemIdLookup = new Dictionary<int, Item>();
    internal Dictionary<string, List<Item>> ItemTypeIndex = new Dictionary<string, List<Item>>();
    private static ItemController _instance;

    private Dictionary<string, Item> _allItemTypes;

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

    internal Dictionary<string, Item> AllItemTypes
    {
        get
        {
            if (_allItemTypes == null)
            {
                _allItemTypes = new Dictionary<string, Item>();
                foreach (var itemFile in FileController.Instance.ItemJson)
                {
                    var item = Instantiate(itemPrefab, transform);

                    item.Load(itemFile.text);
                    item.name = item.Data.Name;

                    AllItemTypes.Add(item.Data.Name, item);
                }
            }
            return _allItemTypes;
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

        ItemTypeIndex[item.Data.ItemType].Remove(item);
        ItemIdLookup.Remove(item.Data.Id);
        ItemDataLookup.Remove(item.Data);

        Destroy(item.gameObject);
    }

    internal ItemData FindClosestItemOfType(CellData centerPoint, string type, bool allowStockpiled)
    {
        if (!ItemTypeIndex.ContainsKey(type) || ItemTypeIndex[type].Count == 0)
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
            foreach (var item in ItemTypeIndex[type])
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

    internal Item GetItem(string name)
    {
        var item = Instantiate(AllItemTypes[name], transform);
        item.Load(FileController.Instance.ItemLookup[name].text);

        item.Data.Id = IdCounter++;
        IndexItem(item);

        return item;
    }

    internal Item LoadItem(ItemData savedItem)
    {
        var item = Instantiate(AllItemTypes[savedItem.Name], transform);
        item.Data = savedItem;
        IndexItem(item);

        return item;
    }

    private void IndexItem(Item item)
    {
        if (!ItemTypeIndex.ContainsKey(item.Data.ItemType))
        {
            ItemTypeIndex.Add(item.Data.ItemType, new List<Item>());
        }

        ItemTypeIndex[item.Data.ItemType].Add(item);
        ItemDataLookup.Add(item.Data, item);
        ItemIdLookup.Add(item.Data.Id, item);

        item.name = $"{item.Data.ItemType} ({item.Data.Id})";
    }
}