using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : IEntity
{
    public bool Buildable;
    public string InUseBy;
    public string Layer;
    public Dictionary<ManaColor, int> ManaValue;
    public string Material;
    public string ShiftX;
    public string ShiftY;
    public string Size;
    public EntityTask Spell;
    public string SpriteName;
    public List<EntityTask> Tasks = new List<EntityTask>();
    public float TravelCost;
    private VisualEffect _outline;

    public Direction Rotation;

    [JsonIgnore]
    private int _width, _height = -1;

    public Structure()
    {
    }

    public Structure(string name, string sprite)
    {
        Name = name;
        SpriteName = sprite;
    }

    internal bool IsInterlocking()
    {
        return IsWall() || IsPipe() || IsPipeEnd();
    }

    public Cell Cell { get; set; }
    public string FactionName { get; set; }

    [JsonIgnore]
    public int Height
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    public string Id { get; set; }

    [JsonIgnore]
    public bool InUseByAnyone
    {
        get
        {
            return !string.IsNullOrEmpty(InUseBy);
        }
    }

    public bool IsBluePrint { get; private set; }
    public ManaPool ManaPool { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public EntityTask Task { get; set; }

    public void RotateCW()
    {
        Rotation = Rotation.Rotate90CW();
        Game.StructureController.RefreshStructure(this);
        Refresh();
    }

    public void RotateCCW()
    {
        Rotation = Rotation.Rotate90CCW();
        Refresh();
    }

    public void Refresh()
    {
        Game.StructureController.RefreshStructure(this);
    }

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.RotateTile(Rotation);
            if (IsWall() || IsPipe())
            {
                tile.sprite = Game.SpriteStore.GetInterlockingSprite(this);
            }
            else
            {
                tile.sprite = Game.SpriteStore.GetSprite(SpriteName);
            }

            if (IsBluePrint)
            {
                tile.color = ColorConstants.BluePrintColor;
            }
            else
            {
                tile.color = Cell.Color;

                if (IsPipe())
                {
                    var pipe = this as Pipe;
                    if (pipe.Attunement.HasValue)
                    {
                        var alpha = ((float)pipe.ManaPool[pipe.Attunement.Value].Total) / 10f;
                        tile.color = pipe.Attunement.Value.GetActualColor(alpha);
                    }
                }
            }

            return tile;
        }
    }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public int Width
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    public static Structure GetFromJson(string json, string name)
    {
        if (name == "Pipe")
        {
            return JsonConvert.DeserializeObject<Pipe>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
            });
        }

        return JsonConvert.DeserializeObject<Structure>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    public void Awake()
    {
        if (ManaPool == null)
        {
            ManaPool = new ManaPool(this);
        }
    }

    public void Damage(int amount, ManaColor type)
    {
        if (!ManaPool.Empty())
        {
            var mana = ManaPool.GetRandomManaColorFromPool();
            ManaPool.BurnMana(mana, amount);
        }
        Game.StructureController.DestroyStructure(this);
    }

    public bool IsPipe()
    {
        return IsType("Pipe");
    }

    public bool IsPipeEnd()
    {
        return IsType("PipeEnd");
    }

    public bool IsType(string name)
    {
        return Properties.ContainsKey("Type") && Properties["Type"].Split(',').Contains(name);
    }

    public bool IsWall()
    {
        return IsType("Wall");
    }

    public void SetBluePrintState(bool state)
    {
        IsBluePrint = state;
        Game.StructureController.RefreshStructure(this);
    }

    public bool ValidateCellLocationForStructure(Cell cell)
    {
        if (!cell.Buildable)
        {
            return false;
        }
        return true;
    }

    internal void Free()
    {
        InUseBy = string.Empty;
    }

    internal void HideOutline()
    {
        if (_outline != null)
        {
            _outline.Kill();
        }
    }

    internal bool IsFloor()
    {
        return IsType("Floor");
    }

    internal void Reserve(string reservedBy)
    {
        InUseBy = reservedBy;
    }

    internal void ShowOutline()
    {
        _outline = Game.EffectController
                       .SpawnSpriteEffect(Cell, "CellOutline", float.MaxValue)
                       .Regular();
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