using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemController : MonoBehaviour
{
    public ItemRenderer ItemPrefab;

    internal Dictionary<string, Item> ItemDataReference = new Dictionary<string, Item>();

    private Dictionary<string, string> _itemTypeFileMap;

    internal Dictionary<string, string> ItemTypeFileMap
    {
        get
        {
            if (_itemTypeFileMap == null)
            {
                _itemTypeFileMap = new Dictionary<string, string>();
                foreach (var itemFile in Game.FileController.ItemFiles)
                {
                    var data = Item.GetFromJson(itemFile.text);
                    ItemTypeFileMap.Add(data.Name, itemFile.text);
                    ItemDataReference.Add(data.Name, data);
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

        var renderer = Instantiate(ItemPrefab, transform);

        var data = Item.GetFromJson(ItemTypeFileMap[name]);
        renderer.Data = data;
        data.Renderer = renderer;

        renderer.SpriteRenderer.sprite = Game.SpriteStore.GetSprite(data.SpriteName);
        IndexItem(data);

        data.Coords = (cell.Vector.x + Random.Range(-0.5f, 0.5f), cell.Vector.y + Random.Range(-0.5f, 0.5f));
        data.Cell = cell;
        data.Amount = amount;
        return data;
    }

    internal void DestroyItem(Item item)
    {
        if (item != null)
        {
            Debug.Log($"Destroying: {item.Name}");

            if (!string.IsNullOrEmpty(item.FactionName))
            {
                Game.FactionController.Factions[item.FactionName].Items.Remove(item);
            }
            IdService.RemoveEntity(item);
            Game.Controller.AddItemToDestroy(item.Renderer.gameObject);
        }
    }

    private void IndexItem(Item item)
    {
        IdService.EnrollEntity(item);
    }
}