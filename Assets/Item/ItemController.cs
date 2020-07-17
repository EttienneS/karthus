using Assets.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Dictionary<string, string> _itemTypeFileMap;
    private Dictionary<string, ItemData> _itemDataReference;

    internal Dictionary<string, ItemData> ItemDataReference
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
                _itemDataReference = new Dictionary<string, ItemData>();
                foreach (var itemFile in Game.Instance.FileController.ItemFiles)
                {
                    try
                    {
                        var data = ItemData.GetFromJson(itemFile.text);
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

    public ItemData SpawnItem(string name, Cell cell, int amount = 1)
    {
        if (!ItemTypeFileMap.ContainsKey(name))
        {
            Debug.LogError($"Item not found: {name}");
        }

        var data = ItemData.GetFromJson(ItemTypeFileMap[name]);
        data.Cell = cell;
        data.Amount = amount;

        SpawnItem(data);
        return data;
    }

    internal void DestroyItem(ItemData item)
    {
        if (item != null)
        {
            Debug.Log($"Destroying: {item.Name}");
            Game.Instance.IdService.RemoveEntity(item);
            Game.Instance.AddItemToDestroy(item.Renderer.gameObject);
        }
    }

    private void IndexItem(ItemData item)
    {
        Game.Instance.IdService.EnrollEntity(item);
    }

    internal void SpawnItem(ItemData data)
    {
        var mesh = Game.Instance.MeshRendererFactory
                                .GetItemMesh(data.Mesh);

        var renderer = Instantiate(mesh, transform).gameObject
                                                   .AddComponent<ItemRenderer>();

        renderer.Data = data;
        data.Renderer = renderer;

        IndexItem(data);

        data.Cell = Game.Instance.Map.GetCellAtCoordinate(new Vector3(data.Coords.X, 0, data.Coords.Z));
        renderer.UpdatePosition();
    }
}