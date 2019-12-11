using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Cell : IEquatable<Cell>
{
    public string FloorId;
    public ManaColor? Liquid;

    [JsonIgnore]
    public Cell[] Neighbors = new Cell[8];

    [JsonIgnore]
    public AutomataState State;

    public string StructureId;
    public int X;
    public int Y;
    internal Color Color;
    private int _biomeId;

    private BiomeRegion _biomeRegion;

    private Structure _floor;

    private float _fluidLevel;

    private Structure _structure;

    public enum AutomataState
    {
        ImmutableDead, MutableDead, ImmutableAlive, MutableAlive
    }

    [JsonIgnore]
    public bool Alive
    {
        get
        {
            return State == AutomataState.ImmutableAlive || State == AutomataState.MutableAlive;
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

    public int BiomeId
    {
        get
        {
            return _biomeId;
        }
        set
        {
            _biomeId = value;
            _biomeRegion = null;
        }
    }

    [JsonIgnore]
    public BiomeRegion BiomeRegion
    {
        get
        {
            if (_biomeRegion == null)
            {
                _biomeRegion = Biome.GetRegion(Height);
            }
            return _biomeRegion;
        }
    }

    [JsonIgnore]
    public bool Buildable
    {
        get
        {
            return TravelCost > 0 && Structure == null;
        }
    }

    [JsonIgnore]
    public List<Creature> Creatures
    {
        get
        {
            return IdService.CreatureLookup.Values.Where(c => c.Cell == this).ToList();
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

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

    public float Height { get; set; }

    [JsonIgnore]
    public bool IsVoid
    {
        get
        {
            return BiomeId == 0;
        }
    }

    public List<string> ItemIds { get; set; } = new List<string>();

    [JsonIgnore]
    public List<Item> Items
    {
        get
        {
            return ItemIds.Select(i => i.GetItem()).ToList();
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
    public Tile LiquidTile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Game.SpriteStore.GetSprite("Liquid");

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

    [JsonIgnore]
    public List<Cell> LivingNeighbours
    {
        get
        {
            return Neighbors.Where(n => n?.Alive == true).ToList();
        }
    }

    [JsonIgnore]
    public Cell NextWithSamePriority { get; set; }

    [JsonIgnore]
    public IEnumerable<Cell> NonNullNeighbors
    {
        get
        {
            return Neighbors.Where(n => n != null);
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
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.RotateRandom90();

            RefreshColor();

            if (Floor == null)
            {
                tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(BiomeRegion.SpriteName);
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

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            return Structure?.IsBluePrint == false ? Structure.TravelCost : BiomeRegion.TravelCost;
        }
    }

    [JsonIgnore]
    public Vector3 Vector
    {
        get
        {
            return new Vector3(X + 0.5f, Y + 0.5f, -1);
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

    public void AddItem(Item item)
    {
        ItemIds.Add(item.Id);
        item.Cell = this;

        item.Coords = (Vector.x, Vector.y);
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

        return neighbor.Structure.IsWall();
    }

    public bool Pathable(Mobility mobility)
    {
        switch (mobility)
        {
            case Mobility.Walk:
                return TravelCost > 0;

            case Mobility.Fly:
                return true;
        }

        return false;
    }

    public void RefreshColor()
    {
        const float totalShade = 1f;
        const float maxShade = 0.4f;
        var baseColor = new Color(totalShade, totalShade, totalShade, 1f);

        var scaled = Helpers.Scale(BiomeRegion.Min, BiomeRegion.Max, 0f, maxShade, Height);

        Color = new Color(baseColor.r - scaled, baseColor.g - scaled, baseColor.b - scaled, baseColor.a);
    }

    public void RunAutomata()
    {
        if (State == AutomataState.ImmutableAlive || State == AutomataState.ImmutableDead)
        {
            return;
        }

        var neigbours = Neighbors.Where(n => n != null);

        var alive = neigbours.Count(n => n.State == AutomataState.ImmutableAlive || n.State == AutomataState.MutableAlive);

        if (alive > 5)
        {
            State = AutomataState.MutableAlive;
        }
        else if (alive < 4)
        {
            State = AutomataState.MutableDead;
        }
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

    public void TakeItem(Item item)
    {
        if (ItemIds.Contains(item.Id))
        {
            ItemIds.Remove(item.Id);
        }
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }

    public void UpdateLiquid()
    {
        Game.Map.LiquidMap.SetTile(new Vector3Int(X, Y, 0), null);

        if (Liquid.HasValue)
        {
            Game.Map.LiquidMap.SetTile(new Vector3Int(X, Y, 0), LiquidTile);
        }
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

    internal void AddLiquid(ManaColor color, float volume)
    {
        Liquid = color;
        LiquidLevel += volume;
        Game.PhysicsController.Track(this);
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

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Game.StructureController.GetStructure(structureName, Game.FactionController.Factions[faction]);
        SetStructure(structure);

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

    internal IEnumerable<Creature> GetEnemyCreaturesOf(string faction)
    {
        return Creatures.Where(c => c.FactionName != faction);
    }

    internal Cell GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    internal void Populate()
    {
        if (Structure?.Name == "Reserved")
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        if (!Empty())
        {
            return;
        }

        var content = BiomeRegion.GetContent();

        if (!string.IsNullOrEmpty(content))
        {
            if (Game.StructureController.StructureDataReference.ContainsKey(content))
            {
                var structure = Game.StructureController.GetStructure(content, Game.FactionController.Factions[FactionConstants.World]);
                SetStructure(structure);

                if (structure.AutoInteractions.Count > 0)
                {
                    Game.MagicController.AddEffector(structure);
                }
            }
            else
            {
                Game.ItemController.SpawnItem(content, this);
            }
        }
    }

    internal Vector3Int ToVector3Int()
    {
        return new Vector3Int(X, Y, 0);
    }

    internal void UpdatePhysics()
    {
        const float minLevel = 0.1f;

        if (LiquidLevel > 0 && IsVoid)
        {
            var level = Random.Range(0.1f, LiquidLevel);

            if (Random.value < level)
            {
                var nonVoid = NonNullNeighbors.Where(n => !n.IsVoid).ToList();
                if (nonVoid.Count > 0)
                {
                    var disintegrated = nonVoid.GetRandomItem();
                    var flow = nonVoid.FirstOrDefault(n => n.Liquid.HasValue && n.Liquid == Liquid);
                    if (flow != null)
                    {
                        disintegrated = flow;
                    }

                    disintegrated.BiomeId = 0;

                    Game.StructureController.DestroyStructure(disintegrated.Structure);
                    Game.StructureController.DestroyStructure(disintegrated.Floor);

                    foreach (var creature in disintegrated.Creatures)
                    {
                        foreach (var limb in creature.Limbs)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                limb.Wounds.Add(new Wound(limb, "Mana explosion", DamageType.Energy, Severity.Critical));
                            }
                        }
                        creature.Log("AAAHAahhaahhahhaaaaaa!!");
                    }
                    disintegrated.UpdateTile();
                    Game.PhysicsController.Track(disintegrated);
                    Game.VisualEffectController.SpawnLightEffect(null, disintegrated.Vector, Color.magenta, 3, 4, 3).Fades();
                }
            }
            else
            {
                Game.VisualEffectController.SpawnLightEffect(null, Vector, Liquid.Value.GetActualColor(), 2, 1, 2).Fades();
            }

            LiquidLevel -= level;
            return;
        }

        if (LiquidLevel <= minLevel || !Liquid.HasValue)
        {
            UpdateLiquid();
            return;
        }

        var clashes = Neighbors.Where(n => n?.Liquid.HasValue == true && n.Liquid.Value != Liquid.Value);
        const float max = 0.02f;
        if (clashes.Any())
        {
            var clash = clashes.GetRandomItem();
            var winner = LiquidLevel > clash.LiquidLevel ? this : clash;
            var loser = winner == this ? clash : this;

            var diff = Mathf.Min(winner.LiquidLevel - loser.LiquidLevel, max);
            winner.LiquidLevel -= diff;
            loser.LiquidLevel -= diff;

            loser.Liquid = null;
            Game.VisualEffectController.SpawnLightEffect(null, loser.Vector, winner.Liquid.Value.GetActualColor(),
                                        1 + (diff * 10f), 1 + (diff * 10f), 2).Fades();

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
        if (Structure == null)
        {
            return false;
        }

        return Structure.IsWall();
    }
}