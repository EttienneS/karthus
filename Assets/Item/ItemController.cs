using System.Collections.Generic;
using UnityEngine;

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

    public Item SpawnItem(string name, Cell cell)
    {
        var renderer = Instantiate(ItemPrefab, transform);
        var data = Item.GetFromJson(ItemTypeFileMap[name]);
        renderer.Data = data;
        data.Renderer = renderer;

        renderer.SpriteRenderer.sprite = Game.SpriteStore.GetSprite(data.SpriteName);
        IndexItem(data);

        data.Cell = cell;
        return data;
    }

    private void IndexItem(Item item)
    {
        IdService.EnrollEntity(item);
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
}