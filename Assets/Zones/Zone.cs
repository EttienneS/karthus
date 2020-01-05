using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zone
{
    public string Name { get; set; }
    public Color Color { get; set; } = ColorExtensions.GetRandomColor();

    public List<Cell> Cells { get; set; } = new List<Cell>();

    // get structures in cells from id service
    public List<Structure> Structures
    {
        get
        {
            var structures = new List<Structure>();

            foreach (var cell in Cells)
            {
                if (IdService.StructureCellLookup.ContainsKey(cell))
                {
                    structures.AddRange(IdService.StructureCellLookup[cell]);
                }
            }

            return structures;
        }
    }

    // get items in cells from id service
    public List<Item> Items
    {
        get
        {
            return IdService.ItemLookup.Values.Where(i => Cells.Contains(i.Cell)).ToList();
        }
    }

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

    public bool CanUse(IEntity entity)
    {
        return string.IsNullOrEmpty(OwnerId) || OwnerId.Equals(entity.Id);
    }
}