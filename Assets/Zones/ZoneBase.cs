﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[JsonConverter(typeof(StringEnumConverter))]
public enum Purpose
{
    Room, Storage, Area
}

public abstract class ZoneBase
{
    public List<Cell> Cells { get; set; } = new List<Cell>();
    public Color Color { get; set; } = ColorExtensions.GetRandomColor();

    // get items in cells from id service
    public List<Item> Items
    {
        get
        {
            return IdService.ItemLookup.Values.Where(i => Cells.Contains(i.Cell)).ToList();
        }
    }

    public string Name { get; set; }

    public IEntity Owner
    {
        get
        {
            if (string.IsNullOrEmpty(OwnerId))
            {
                return null;
            }

            return IdService.GetEntity(OwnerId);
        }
    }

    public string OwnerId { get; set; }

    public string FactionName { get; set; }

    // get structures in cells from id service
    public List<Structure> Structures
    {
        get
        {
            var structures = new List<Structure>();

            foreach (var cell in Cells.Where(c => IdService.StructureCellLookup.ContainsKey(c)))
            {
                structures.AddRange(IdService.StructureCellLookup[cell]);
            }

            return structures;
        }
    }

    public bool CanUse(IEntity entity)
    {
        return string.IsNullOrEmpty(OwnerId) || OwnerId.Equals(entity.Id);
    }
}