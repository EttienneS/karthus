using Assets.Item;
using Assets.Map;
using Assets.ServiceLocator;
using Assets.ServiceLocator;
using Assets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ItemController : MonoBehaviour, IGameService
{
    internal Dictionary<string, ItemData> ItemDataReference { get; set; }

    internal Dictionary<string, string> ItemTypeFileMap { get; set; }

    public void Initialize()
    {
        ItemTypeFileMap = new Dictionary<string, string>();
        ItemDataReference = new Dictionary<string, ItemData>();
        foreach (var itemFile in Loc.GetFileController().ItemFiles)
        {
            try
            {
                var data = ItemData.GetFromJson(itemFile.text);
                ItemTypeFileMap.Add(data.Name, itemFile.text);
                ItemDataReference.Add(data.Name, data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to load item {itemFile}: {ex.Message}");
            }
        }
    }

    public ItemData SpawnItem(string name, Cell cell, int amount = 1, bool automerge = true)
    {
        if (!ItemTypeFileMap.ContainsKey(name))
        {
            Debug.LogError($"Item not found: {name}");
        }

        if (automerge)
        {
            var itemToMerge = cell.Items.FirstOrDefault(i => i.Name == name);
            if (itemToMerge != null)
            {
                itemToMerge.Amount += amount;
                return itemToMerge;
            }
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
            Loc.GetIdService().RemoveItem(item);
            Loc.GetGameController().AddItemToDestroy(item.Renderer.gameObject);
        }
    }

    internal List<FilterViewOption> GetAllItemOptions()
    {
        var options = new List<FilterViewOption>();
        foreach (var item in ItemDataReference.Values)
        {
            options.Add(new FilterViewOption(item.Name, Loc.GetSpriteStore().GetSprite(item.Icon), item.Categories));
        }
        return options;
    }
    internal void SpawnItem(ItemData data)
    {
        var mesh = Loc.GetGameController().MeshRendererFactory
                                .GetItemMesh(data.Mesh);

        var meshObject = Instantiate(mesh, transform);

        var itemRenderer = meshObject.gameObject.AddComponent<ItemRenderer>();

        itemRenderer.Data = data;
        data.Renderer = itemRenderer;

        IndexItem(data);

        data.Cell = Loc.GetMap().GetCellAtCoordinate(new Vector3(data.Coords.X, 0, data.Coords.Z));
        itemRenderer.UpdatePosition();
    }

    private void IndexItem(ItemData item)
    {
        Loc.GetIdService().EnrollItem(item);
    }
}