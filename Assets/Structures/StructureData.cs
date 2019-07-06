using Newtonsoft.Json;
using System.Collections.Generic;

public class StructureData : IMagicAttuned
{
    public TaskBase Behaviour;

    public bool Buildable;

    public Coordinates Coordinates;

    public string FactionName;
    public int Id;

    public string InUseBy;

    public bool IsBluePrint;

    public string Layer;

    public Dictionary<ManaColor, int> ManaValue;
    public string Material;
    public string Name;

    public Dictionary<string, string> Properties = new Dictionary<string, string>();

    public string ShiftX;
    public string ShiftY;
    public string Size;
    public string SpriteName;
    public string StructureType;
    public List<TaskBase> Tasks = new List<TaskBase>();
    public bool Tiled;
    public float TravelCost;

    [JsonIgnore]
    private int _width, _height = -1;

    public StructureData()
    {
    }

    public StructureData(string name, string sprite)
    {
        Name = name;
        SpriteName = sprite;
    }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            return FactionController.Factions[FactionName];
        }
    }

    [JsonIgnore]
    public int Height
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    [JsonIgnore]
    public bool InUseByAnyone
    {
        get
        {
            return !string.IsNullOrEmpty(InUseBy);
        }
    }

    [JsonIgnore]
    public Structure LinkedGameObject
    {
        get
        {
            return Game.StructureController.GetStructureForData(this);
        }
    }

    public ManaPool ManaPool { get; set; } = new ManaPool();

    [JsonIgnore]
    public int Width
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    public static StructureData GetFromJson(string json)
    {
        return JsonConvert.DeserializeObject<StructureData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    public List<CellData> GetCellsForStructure(Coordinates origin)
    {
        List<CellData> cells = new List<CellData>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells.Add(Game.MapGrid.GetCellAtCoordinate(new Coordinates(origin.X + x, origin.Y + y)));
            }
        }

        return cells;
    }

    public void SetBlueprintState(bool blueprint)
    {
        if (blueprint)
        {
            LinkedGameObject.SpriteRenderer.color = ColorConstants.BluePrintColor;
            IsBluePrint = true;
        }
        else
        {
            LinkedGameObject.SpriteRenderer.color = ColorConstants.BaseColor;
            IsBluePrint = false;
        }
    }

    public bool ValidateCellLocationForStructure(CellData CellData)
    {
        foreach (var cell in GetCellsForStructure(CellData.Coordinates))
        {
            if (!cell.Buildable)
            {
                return false;
            }
        }
        return true;
    }

    internal void Free()
    {
        InUseBy = string.Empty;
    }

    internal void Reserve(string reservedBy)
    {
        InUseBy = reservedBy;
    }

    private void ParseHeight()
    {
        if (_width == -1 || _height == -1)
        {
            if (!string.IsNullOrEmpty(Size))
            {
                var parts = Size.Split('x');
                _height = int.Parse(parts[0]);
                _width = int.Parse(parts[1]);
            }
            else
            {
                _width = 1;
                _height = 1;
            }
        }
    }
}