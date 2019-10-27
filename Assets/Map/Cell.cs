using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Cell : IEquatable<Cell>
{
    public string Binder;

    [JsonIgnore]
    public CellType CellType
    {
        get
        {
            return Game.MapGenerator.Biomes[BiomeId].GetCellType(_height);
        }
    }

    public List<string> CreatureIds = new List<string>();

    public string FloorId;

    [JsonIgnore]
    public Cell[] Neighbors = new Cell[8];

    public Direction Rotation;

    public string StructureId;

    public int X;

    public int Y;

    internal Color Color;

    private IEntity _binding;

    private Structure _floor;

    private float _fluidLevel;

    private float _height;

    private Structure _structure;

    [JsonIgnore]
    public IEntity Binding
    {
        get
        {
            if (_binding == null && !string.IsNullOrEmpty(Binder))
            {
                _binding = Binder.GetEntity();
            }

            return _binding;
        }
        set
        {
            _binding = value;
            if (_binding == null)
            {
                Binder = string.Empty;
            }
            else
            {
                Binder = _binding.Id;
            }
        }
    }

    [JsonIgnore]
    public bool Bound
    {
        get
        {
            return Binding != null;
        }
    }

    [JsonIgnore]
    public bool Buildable
    {
        get
        {
            return Bound && TravelCost > 0 && Structure == null;
        }
    }

    internal void Unbind()
    {
        Binding = null;
        UpdateTile();
        Game.VisualEffectController.SpawnLightEffect(null, this, Color.magenta, 1.5f, 8, 8)
                                   .Fades();
    }

    internal void Bind(IEntity entity)
    {
        Binding = entity;
        UpdateTile();
        Game.VisualEffectController.SpawnLightEffect(null, this, Color.magenta, 2, 4, 5)
                                   .Fades();
    }

    [JsonIgnore]
    public List<CreatureData> Creatures
    {
        get
        {
            return CreatureIds.Select(c => c.GetCreature()).ToList();
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    public bool DrawnOnce { get; set; }

    [JsonIgnore]
    public Structure Floor
    {
        get
        {
            if (_floor == null && !string.IsNullOrEmpty(FloorId))
            {
                _floor = FloorId.GetStructure();
            }
            return _floor;
        }
        set
        {
            _floor = value;

            if (_floor == null)
            {
                FloorId = string.Empty;
            }
            else
            {
                FloorId = _floor.Id;
            }
        }
    }

    public float LiquidLevel
    {
        get
        {
            return _fluidLevel;
        }
        set
        {
            _fluidLevel = value;
            if (_fluidLevel < 0)
            {
                _fluidLevel = 0;
            }
            Game.PhysicsController.Track(this);
        }
    }

    [JsonIgnore]
    public Biome Biome
    {
        get
        {
            return Game.MapGenerator.Biomes[BiomeId];
        }
    }

    public int BiomeId = 0;

    public float Height
    {
        get
        {
            return _height;
        }
        set
        {
            _height = value;
        }
    }

    //[JsonIgnore]
    //public float Level
    //{
    //    get
    //    {
    //        var height = Height + FluidLevel;
    //        if (Structure?.IsWall() == true)
    //        {
    //            height += 2;
    //        }

    //        return height;
    //    }
    //}

    [JsonIgnore]
    public Cell NextWithSamePriority { get; set; }

    [JsonIgnore]
    public bool Pathable
    {
        get
        {
            return Bound && TravelCost > 0;
        }
    }

    [JsonIgnore]
    public Cell PathFrom { get; set; }

    [JsonIgnore]
    public int SearchHeuristic { private get; set; }

    [JsonIgnore]
    public int SearchPhase { get; set; }

    [JsonIgnore]
    public int SearchPriority => (int)Distance + SearchHeuristic;

    [JsonIgnore]
    public Structure Structure
    {
        get
        {
            if (_structure == null && !string.IsNullOrEmpty(StructureId))
            {
                _structure = StructureId.GetStructure();
            }

            return _structure;
        }
        set
        {
            _structure = value;

            if (_structure == null)
            {
                StructureId = string.Empty;
            }
            else
            {
                StructureId = _structure.Id;
            }
        }
    }

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            DrawnOnce = true;
            var tile = ScriptableObject.CreateInstance<Tile>();
            RefreshColor();

            if (Floor == null)
            {
                tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(CellType);
                tile.color = Color;
            }
            else
            {
                tile.sprite = Game.SpriteStore.GetSprite(Floor.SpriteName);
                var col = Color;

                if (Floor.IsBluePrint)
                {
                    col = ColorConstants.BluePrintColor;
                    col.a = 1;
                }

                tile.color = col;
            }

            return tile;
        }
    }

    public ManaColor? Liquid;

    [JsonIgnore]
    public Tile LiquidTile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(CellType.Void);

            if (Liquid.HasValue && LiquidLevel > 0)
            {
                tile.color = Liquid.Value.GetActualColor(Mathf.Max(LiquidLevel, 0.2f));
            }
            else
            {
                tile.color = new Color(0, 0, 0, 0);
            }

            return tile;
        }
    }

    internal void AddLiquid(ManaColor color, float volume)
    {
        Liquid = color;
        LiquidLevel += volume;
    }

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            switch (CellType)
            {
                case CellType.Water:
                case CellType.Mountain:
                    return -1;
            }

            return Structure != null && !Structure.IsBluePrint ? Structure.TravelCost : 1.5f;
        }
    }

    internal bool HasBuilding
    {
        get
        {
            return Structure != null || Floor != null;
        }
    }

    public static Cell FromPosition(Vector2 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector2(0.5f, 0.5f);
        return Game.Map.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public static bool operator !=(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return !ReferenceEquals(obj2, null);
        }

        return !obj1.Equals(obj2);
    }

    public static bool operator ==(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return ReferenceEquals(obj2, null);
        }

        return obj1.Equals(obj2);
    }

    public void AddCreature(CreatureData creature)
    {
        CreatureIds.Add(creature.Id);
    }

    public int DistanceTo(Cell other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public bool Equals(Cell other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Cell;
        if (other == null)
        {
            return false;
        }

        return this == other;
    }

    public override int GetHashCode()
    {
        return $"{X}:{Y}".GetHashCode();
    }

    public Cell GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public bool IsInterlocking(Direction direction)
    {
        var neighbor = Neighbors[(int)direction];

        if (neighbor == null)
        {
            return false;
        }

        if (neighbor.Structure == null)
        {
            return false;
        }

        return neighbor.Structure.IsWall() || neighbor.Structure.IsPipe() || neighbor.Structure.IsPipeEnd();
    }

    public void RefreshColor()
    {
        const float totalShade = 1f;
        const float maxShade = 0.4f;
        var baseColor = new Color(totalShade, Bound ? totalShade : 0.6f, totalShade, Bound ? 1f : 0.6f);

        var range = Game.MapGenerator.Biomes[BiomeId].GetCellTypeRange(CellType);
        var scaled = Helpers.Scale(range.Item1, range.Item2, 0f, maxShade, Height);

        Color = new Color(baseColor.r - scaled, baseColor.g - scaled, baseColor.b - scaled, baseColor.a);
    }

    public void RemoveCreature(CreatureData creature)
    {
        CreatureIds.Remove(creature.Id);
    }

    public void RotateCCW()
    {
        Rotation = Rotation.Rotate90CCW();
        UpdateTile();
    }

    public void RotateCW()
    {
        Rotation = Rotation.Rotate90CW();
        UpdateTile();
    }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public void SetStructure(Structure structure)
    {
        if (structure.IsFloor())
        {
            if (Floor != null)
            {
                Game.StructureController.DestroyStructure(Floor);
            }
            structure.Cell = this;
            Floor = structure;
            UpdateTile();
        }
        else
        {
            if (Structure != null)
            {
                Game.StructureController.DestroyStructure(Structure);
            }

            structure.Cell = this;
            Structure = structure;
        }
    }

    public Vector2 ToMapVector()
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        return new Vector2(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f);
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }

    public Vector3 ToTopOfMapVector()
    {
        return new Vector3(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f, -1);
    }

    public void UpdateTile()
    {
        Game.Map.Tilemap.SetTile(new Vector3Int(X, Y, 0), null);
        Game.Map.Tilemap.SetTile(new Vector3Int(X, Y, 0), Tile);

        if (Structure != null)
        {
            Game.StructureController.RefreshStructure(Structure);
        }
    }

    public void UpdateLiquid()
    {
        Game.Map.LiquidMap.SetTile(new Vector3Int(X, Y, 0), null);

        if (Liquid.HasValue)
        {
            Game.Map.LiquidMap.SetTile(new Vector3Int(X, Y, 0), LiquidTile);
        }
    }

    internal void Clear()
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        if (Floor != null)
        {
            Game.StructureController.DestroyStructure(Floor);
        }
    }

    internal int CountNeighborsOfType(CellType? cellType)
    {
        if (!cellType.HasValue)
        {
            return Neighbors.Count(n => n == null);
        }

        return Neighbors.Count(n => n != null && n.CellType == cellType.Value);
    }

    internal Structure CreateStructure(string structureName, bool bind = false, string faction = FactionConstants.World)
    {
        var structure = Game.StructureController.GetStructure(structureName, Game.FactionController.Factions[faction]);
        SetStructure(structure);

        if (bind)
        {
            Bind(structure);
        }

        if (structure.AutoInteractions.Count > 0)
        {
            Game.MagicController.AddEffector(structure);
        }

        return structure;
    }

    internal bool Empty()
    {
        return Structure == null && Floor == null;
    }

    internal IEnumerable<CreatureData> GetEnemyCreaturesOf(string faction)
    {
        return Creatures.Where(c => c.FactionName != faction);
    }

    internal Cell GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    internal Vector3Int ToVector3Int()
    {
        return new Vector3Int(X, Y, 0);
    }

    internal void UpdatePhysics()
    {
        const float minLevel = 0.1f;
        if (LiquidLevel <= minLevel || !Liquid.HasValue)
        {
            UpdateLiquid();
            return;
        }

        var clash = Array.Find(Neighbors, n => n?.Liquid.HasValue == true && n.Liquid.Value != Liquid.Value);
        const float max = 0.02f;
        if (clash != null)
        {
            var winner = LiquidLevel > clash.LiquidLevel ? this : clash;
            var loser = winner == this ? clash : this;

            var diff = Mathf.Min(winner.LiquidLevel - loser.LiquidLevel, max);
            winner.LiquidLevel -= diff;
            loser.LiquidLevel -= diff;

            loser.Liquid = null;
            Game.VisualEffectController.SpawnLightEffect(null, loser, winner.Liquid.Value.GetActualColor(),
                                        1 + diff, 1 + diff, 2).Fades();

            winner.UpdateLiquid();
            loser.UpdateLiquid();
        }
        else
        {
            var drop = Mathf.Max(minLevel, LiquidLevel / 2);
            var options = Neighbors.Where(n => n?.BlocksFluid() == false
                                               && n.LiquidLevel < LiquidLevel).ToList();

            if (options.Count > 0)
            {
                var lower = options.GetRandomItem();
                lower.LiquidLevel += drop;
                lower.Liquid = Liquid;
                LiquidLevel -= drop;
                UpdateLiquid();
            }
        }
    }

    private bool BlocksFluid()
    {
        if (!Bound)
        {
            return true;
        }

        if (Structure == null)
        {
            return false;
        }

        return Structure.IsWall();
    }
}