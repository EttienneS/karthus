using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : IEntity
{
    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    public List<EffectBase> ActivatedInteractions = new List<EffectBase>();

    public List<EffectBase> AutoInteractions = new List<EffectBase>();

    public bool Buildable;

    // rather than serialzing the cell object we keep this lazy link for load
    public (int X, int Y) Coords = (-1, -1);

    public IEntity InUseBy;
    public string Layer;
    public Dictionary<ManaColor, int> ManaValue;
    public Direction Rotation;
    public string ShiftX;
    public string ShiftY;
    public string Size;
    public string SpriteName;
    public float TravelCost;
    private Cell _cell;
    private Faction _faction;
    private VisualEffect _outline;

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

    [JsonIgnore]
    public Cell Cell
    {
        get
        {
            if (_cell == null && Coords.X >= 0 && Coords.Y >= 0)
            {
                _cell = Game.Map.GetCellAtCoordinate(Coords.X, Coords.Y);
            }
            return _cell;
        }
        set
        {
            if (value != null)
            {
                _cell = value;
                Coords = (_cell.X, _cell.Y);
            }
        }
    }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = Game.FactionController.Factions[FactionName];
            }
            return _faction;
        }
    }

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
            return InUseBy != null;
        }
    }

    public bool IsBluePrint { get; private set; }

    public ManaPool ManaPool { get; set; }

    public string Name { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

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
            return json.LoadJson<Pipe>();
        }

        return json.LoadJson<Structure>();
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

    public void Refresh()
    {
        Game.StructureController.RefreshStructure(this);
    }

    public void RotateCCW()
    {
        Rotation = Rotation.Rotate90CCW();
        Refresh();
    }

    public void RotateCW()
    {
        Rotation = Rotation.Rotate90CW();
        Game.StructureController.RefreshStructure(this);
        Refresh();
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
        InUseBy = null;
    }

    internal List<EffectBase> GetInteraction()
    {
        var interactions = new List<EffectBase>();
        foreach (var interaction in AutoInteractions.Where(i => !i.Disabled))
        {
            interaction.AssignedEntityId = this.Id;
            interactions.Add(interaction);
        }

        return interactions;
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

    internal bool IsInterlocking()
    {
        return IsWall() || IsPipe() || IsPipeEnd();
    }

    internal void Reserve(IEntity reservedBy)
    {
        InUseBy = reservedBy;
    }

    internal void ShowOutline()
    {
        _outline = Game.VisualEffectController
                       .SpawnSpriteEffect(this, Cell, "CellOutline", float.MaxValue);
        _outline.Regular();
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