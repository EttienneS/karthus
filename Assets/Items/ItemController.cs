using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item itemPrefab;

    private static ItemController _instance;

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

    internal Item GetItem()
    {
        return Instantiate(itemPrefab, transform);
    }
}
