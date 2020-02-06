using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum Purpose
{
    Room, Storage, Area
}

public abstract class ZoneBase
{
    [JsonIgnore]
    private List<Cell> _cells;

    public Purpose Purpose;

    [JsonIgnore]
    public List<Cell> Cells
    {
        get
        {
            if (_cells == null)
            {
                _cells = new List<Cell>();
            }
            return _cells;
        }
        set
        {
            _cells = value;
        }
    }

    public string CellString
    {
        get
        {
            return _cells.Select(c => c.X + ":" + c.Y).Aggregate((s1, s2) => s1 + "," + s2);
        }
        set
        {
            _cells = new List<Cell>();
            foreach (var xy in value.Split(','))
            {
                var split = xy.Split(':').Select(i => float.Parse(i)).ToList();
                _cells.Add(Game.Map.GetCellAtCoordinate(split[0], split.Last()));
            }
        }
    }

    [JsonIgnore]
    public float[] Color { get; set; } = ColorExtensions.GetRandomColor().ToFloatArray();

    [JsonIgnore]
    // get items in cells from id service
    public List<Item> Items
    {
        get
        {
            return Game.IdService.ItemLookup.Values.Where(i => _cells.Contains(i.Cell)).ToList();
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

    public string FactionName { get; set; }

    [JsonIgnore]
    // get structures in cells from id service
    public List<Structure> Structures
    {
        get
        {
            var structures = new List<Structure>();

            foreach (var cell in _cells.Where(c => Game.IdService.StructureCellLookup.ContainsKey(c)))
            {
                structures.AddRange(Game.IdService.StructureCellLookup[cell]);
            }

            return structures;
        }
    }

    public bool CanUse(IEntity entity)
    {
        return string.IsNullOrEmpty(OwnerId) || OwnerId.Equals(entity.Id);
    }
}