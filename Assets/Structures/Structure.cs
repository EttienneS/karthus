using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : IEntity
{
    public List<EffectBase> AutoInteractions = new List<EffectBase>();

    public bool Buildable;

    // rather than serializing the cell object we keep this lazy link for load
    public (int X, int Y) Coords = (-1, -1);

    [JsonIgnore]
    public IEntity InUseBy
    {
        get
        {
            if (string.IsNullOrEmpty(InUseById))
            {
                return null;
            }
            return InUseById.GetEntity();
        }
        set
        {
            InUseById = value.Id;
        }
    }

    internal float GetValue(string valueName)
    {
        if (ValueProperties.ContainsKey(valueName))
        {
            return ValueProperties[valueName];
        }
        return 0f;
    }

    internal void SetValue(string valueName, float value)
    {
        if (!ValueProperties.ContainsKey(valueName))
        {
            ValueProperties.Add(valueName, 0f);
        }
        ValueProperties[valueName] = value;
    }

    internal string GetProperty(string propertyName)
    {
        if (Properties.ContainsKey(propertyName))
        {
            return Properties[propertyName];
        }
        return string.Empty;
    }

    internal void SetProperty(string propertyName, string value)
    {
        if (!Properties.ContainsKey(propertyName))
        {
            Properties.Add(propertyName, string.Empty);
        }
        Properties[propertyName] = value;
    }

    public string InUseById { get; set; }

    public string Layer;

    public Direction Rotation;

    public string Size;
    public Cost Cost { get; set; } = new Cost();

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

    public float HP { get; set; } = 5;

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

    internal bool CanHold(Item item)
    {
        var capacity = Mathf.FloorToInt(GetValue(NamedProperties.Capacity));
        var containedItemType = GetProperty(NamedProperties.ContainedItemType);
        var containedItemCount = GetValue(NamedProperties.ContainedItemCount);

        if (string.IsNullOrEmpty(containedItemType) || containedItemType == item.Name)
        {
            return containedItemCount + item.Amount <= capacity;
        }

        if (!string.IsNullOrEmpty(containedItemType) && containedItemType != item.Name)
        {
            return false;
        }

        var filter = Helpers.WildcardToRegex(GetProperty(NamedProperties.Filter));
        return Regex.IsMatch(item.Name, filter);
    }

    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    public string Name { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.RotateTile(Rotation);

            //if (!Buildable)
            //{
            //    tile.ShiftTile(new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)));
            //}

            if (IsWall())
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
            }

            return tile;
        }
    }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public Vector2 Vector
    {
        get
        {
            return Cell.Vector;
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

    public string Description { get; set; }

    public static Structure GetFromJson(string json)
    {
        return json.LoadJson<Structure>();
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
        return IsWall();
    }

    internal void Reserve(IEntity reservedBy)
    {
        InUseBy = reservedBy;
    }

    internal void ShowOutline()
    {
        _outline = Game.VisualEffectController
                       .SpawnSpriteEffect(this, Vector, "CellOutline", float.MaxValue);
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

    public int GetItemCount(string itemType)
    {
        if (GetProperty(NamedProperties.ContainedItemType) == itemType)
        {
            return Mathf.FloorToInt(GetValue(NamedProperties.ContainedItemCount));
        }
        return 0;
    }

    internal Item GetItem(string itemType, int amount)
    {
        var containedType = GetProperty(NamedProperties.ContainedItemType);
        var currentCount = Mathf.FloorToInt(GetValue(NamedProperties.ContainedItemCount));

        if (containedType != itemType)
        {
            return null;
        }

        if (currentCount <= amount)
        {
            amount = currentCount;
        }

        var item = Game.ItemController.SpawnItem(itemType, Cell, amount);
        currentCount -= amount;

        if (currentCount > 0)
        {
            SetValue(NamedProperties.ContainedItemCount, currentCount);
        }
        else
        {
            ContainedItemEffect?.DestroySelf();
            SetValue(NamedProperties.ContainedItemCount, 0);
            SetProperty(NamedProperties.ContainedItemType, string.Empty);
        }

        return item;
    }

    [JsonIgnore]
    public VisualEffect ContainedItemEffect;

    internal bool AddItem(Item item)
    {
        item.Coords = (Cell.Vector.x, Cell.Vector.y);

        if (IsContainer())
        {
            var capacity = Mathf.FloorToInt(GetValue(NamedProperties.Capacity));
            var containedType = GetProperty(NamedProperties.ContainedItemType);
            var currentCount = GetValue(NamedProperties.ContainedItemCount);
            if (string.IsNullOrEmpty(containedType))
            {
                SetProperty(NamedProperties.ContainedItemType, item.Name);
                SetValue(NamedProperties.ContainedItemCount, currentCount + item.Amount);
                Game.ItemController.DestroyItem(item);
                ContainedItemEffect = Game.VisualEffectController.SpawnSpriteEffect(this, Vector, item.SpriteName, float.MaxValue);
                return true;
            }
            else
            {
                if (containedType != item.Name)
                {
                    return false;
                }

                var remainingCapacity = Mathf.FloorToInt(capacity - currentCount);
                if (item.Amount < remainingCapacity)
                {
                    SetValue(NamedProperties.ContainedItemCount, currentCount + item.Amount);
                    Game.ItemController.DestroyItem(item);
                    return true;
                }
                else
                {
                    SetValue(NamedProperties.ContainedItemCount, capacity);
                    item.Amount -= remainingCapacity;
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsContainer()
    {
        return IsType(NamedProperties.Container);
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}