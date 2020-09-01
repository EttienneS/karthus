using Assets;
using Assets.Item;
using Assets.Structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public abstract class ZoneBase
{
    public ManagedCellCollection ZoneCells = new ManagedCellCollection();

    [JsonIgnore]
    public string ColorString { get; set; } = ColorExtensions.GetRandomColor().ToColorHexString();

    public string FactionName { get; set; }

    [JsonIgnore]
    public List<ItemData> Items
    {
        get
        {
            return Game.Instance.IdService.ItemLookup.Values.Where(i => ZoneCells.GetCells().Contains(i.Cell)).ToList();
        }
    }

    public string Name { get; set; }

    [JsonIgnore]
    public IEntity Owner
    {
        get
        {
            if (string.IsNullOrEmpty(OwnerId))
            {
                return null;
            }

            return OwnerId.GetEntity();
        }
    }

    public string OwnerId { get; set; }

    [JsonIgnore]
    // get structures in cells from id service
    public List<Structure> Structures
    {
        get
        {
            var structures = new List<Structure>();

            foreach (var cell in ZoneCells.GetCells().Where(c => Game.Instance.IdService.StructureCellLookup.ContainsKey(c)))
            {
                structures.AddRange(Game.Instance.IdService.StructureCellLookup[cell]);
            }

            return structures;
        }
    }

    public bool CanUse(IEntity entity)
    {
        return string.IsNullOrEmpty(OwnerId) || OwnerId.Equals(entity.Id);
    }
}