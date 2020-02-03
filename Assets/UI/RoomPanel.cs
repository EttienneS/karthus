﻿using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    internal RoomZone Zone;

    public Text RoomInfo;

    public void Update()
    {
        RoomInfo.text = "Zone Info:\n\n";
        RoomInfo.text += $"Size: {Zone.Cells.Count}\n\nStructures:\n\n";

        foreach (var structure in Zone.Structures)
        {
            RoomInfo.text += $"{structure.Name}";

            if (structure is Container container)
            {
                RoomInfo.text += $": {container.ItemType} {container.Count}/{container.Capacity}";
            }

            RoomInfo.text += "\n";
        }

        RoomInfo.text += $"\nItems:\n\n";

        foreach (var item in Zone.Items)
        {
            RoomInfo.text += $"{item.Name}: {item.Amount}\n";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(RoomZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;
    }
}