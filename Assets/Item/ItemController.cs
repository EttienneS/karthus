using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemController : MonoBehaviour
{
    public ItemRenderer ItemPrefab;

    private Dictionary<string, string> _itemTypeFileMap;
    private Dictionary<string, Item> _itemDataReference;

    internal Dictionary<string, Item> ItemDataReference
    {
        get
        {
            ItemTypeFileMap.First();
            return _itemDataReference;
        }
    }

    internal Dictionary<string, string> ItemTypeFileMap
    {
        get
        {
            if (_itemTypeFileMap == null)
            {
                _itemTypeFileMap = new Dictionary<string, string>();
                _itemDataReference = new Dictionary<string, Item>();
                foreach (var itemFile in Game.FileController.ItemFiles)
                {
                    try
                    {
                        var data = Item.GetFromJson(itemFile.text);
                        _itemTypeFileMap.Add(data.Name, itemFile.text);
                        _itemDataReference.Add(data.Name, data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Unable to load item {itemFile}: {ex.Message}");
                    }
                }
            }
            return _itemTypeFileMap;
        }
    }

    public Item SpawnItem(string name, Cell cell, int amount = 1)
    {
        if (!ItemTypeFileMap.ContainsKey(name))
        {
            Debug.LogError($"Item not found: {name}");
        }

        var data = Item.GetFromJson(ItemTypeFileMap[name]);
        data.Cell = cell;
        data.Amount = amount;

        SpawnItem(data);
        return data;
    }

    internal void DestroyItem(Item item)
    {
        if (item != null)
        {
            Debug.Log($"Destroying: {item.Name}");
            Game.IdService.RemoveEntity(item);
            Game.Instance.AddItemToDestroy(item.Renderer.gameObject);
        }
    }

    private void IndexItem(Item item)
    {
        Game.IdService.EnrollEntity(item);
    }

    internal void SpawnItem(Item data)
    {
        var renderer = Instantiate(ItemPrefab, transform);

        renderer.Data = data;
        data.Renderer = renderer;

        renderer.SpriteRenderer.sprite = Game.SpriteStore.GetSprite(data.SpriteName);
        renderer.UpdatePosition();

        IndexItem(data);

        data.Cell = Game.Map.GetCellAtCoordinate(new Vector2(data.Coords.X, data.Coords.Y));
    }
}