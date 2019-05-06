using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    internal StructureData Data = new StructureData();
    internal SpriteRenderer SpriteRenderer;
    internal SpriteRenderer StatusSprite;

    public void LoadSprite()
    {
        SpriteRenderer.sprite = Game.SpriteStore.GetSpriteByName(Data.SpriteName);
        SetTiledMode(SpriteRenderer, Data.Tiled);
    }

    public void SetTiledMode(SpriteRenderer spriteRenderer, bool tiled)
    {
        if (!tiled)
        {
            spriteRenderer.drawMode = SpriteDrawMode.Simple;
        }
        else
        {
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = Vector2.one;
        }

        spriteRenderer.transform.localScale = new Vector3(Data.Width, Data.Height, 1);
    }

    internal void Load(string structureData)
    {
        Data = StructureData.GetFromJson(structureData);
        LoadSprite();
    }

    internal void Shift()
    {
        if (!string.IsNullOrEmpty(Data.ShiftX))
        {
            float shiftX = Helpers.GetValueFromFloatRange(Data.ShiftX);
            transform.position += new Vector3(shiftX, 0, 0);
        }

        if (!string.IsNullOrEmpty(Data.ShiftY))
        {
            float shiftY = Helpers.GetValueFromFloatRange(Data.ShiftY);
            transform.position += new Vector3(0, shiftY, 0);
        }

        if (Data.Width > 1 || Data.Height > 1)
        {
            transform.position += new Vector3((Data.Width / 2) - 0.5f, (Data.Height / 2) - 0.5f, 0);
        }
    }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        StatusSprite = transform.Find("Status").GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Game.TimeManager.Paused) return;

        if (!Data.IsBluePrint && Data.Behaviour != null)
        {
            Data.Behaviour.Originator = Data.GetGameId();
            if (!Data.Behaviour.Done())
            {
                Data.Behaviour.Update();
            }
        }

        if (Data.IsBluePrint && !Factions.Taskmasters[Data.Faction].Tasks.OfType<Build>().Any(t => t.Structure == Data))
        {
            Factions.Taskmasters[Data.Faction].AddTask(new Build(Data, Data.Coordinates), Data.GetGameId());
        }
    }
}

public class StructureData
{
    public TaskBase Behaviour;

    public bool Buildable;

    public Coordinates Coordinates;

    public int Id;

    public string InUseBy;

    public bool IsBluePrint;

    public string Layer;

    public string Name;

    public Dictionary<string, string> Properties = new Dictionary<string, string>();

    public List<string> RequireStrings;

    public string Faction = FactionConstants.World;

    public string ShiftX;

    public string ShiftY;

    public string Size;

    public string SpriteName;

    public string StructureType;

    public List<TaskBase> Tasks = new List<TaskBase>();

    public bool Tiled;

    public float TravelCost;

    public List<string> Yield;

    private List<string> _require;

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

    [JsonIgnore]
    public List<string> Require
    {
        get
        {
            if (_require == null)
            {
                _require = new List<string>();
                foreach (var reqString in RequireStrings)
                {
                    _require.AddRange(Helpers.ParseItemString(reqString));
                }
            }
            return _require;
        }
    }

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

    public void AddItem(ItemData item)
    {
        if (Require.Contains(item.Name))
        {
            Require.Remove(item.Name);
        }
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

    public void SpawnYield(CellData cell)
    {
        foreach (var yieldString in Yield)
        {
            foreach (var item in Helpers.ParseItemString(yieldString))
            {
                cell.AddContent(Game.ItemController.GetItem(item).gameObject);
            }
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