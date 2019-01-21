using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item itemPrefab;

    private static ItemController _instance;

    internal Dictionary<string, Item> AllItems = new Dictionary<string, Item>();

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

            AllItems.Add(item.Data.Name, item);
        }
    }

    internal Item GetItem(string name)
    {
        return Instantiate(AllItems[name], transform);
    }

    internal void DestoyItem(Item item)
    {
        Destroy(item.gameObject);
    }
}