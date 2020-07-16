using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Structures
{
    public class Structure : IEntity
    {
        public bool Buildable;

        public string ColorHex = "#ffffff";

        [JsonIgnore]
        public VisualEffect ContainedItemEffect;

        // rather than serializing the cell object we keep this lazy link for load
        public (int X, int Y) Coords = (-1, -1);

        public string Icon;
        public string Layer;

        public string Mesh;
        public float Rotation;

        public bool SpawnRotation;
        public float TravelCost;

        public string Type;
        private Cell _cell;

        private Faction _faction;

        private VisualEffect _outline;

        public Structure()
        {
        }

        public Structure(string name, string mesh)
        {
            Name = name;
            Mesh = mesh;
        }

        [JsonIgnore]
        public Cell Cell
        {
            get
            {
                if (_cell == null && Coords.X >= 0 && Coords.Y >= 0)
                {
                    _cell = Game.Instance.Map.GetCellAtCoordinate(Coords.X, Coords.Y);
                }
                return _cell;
            }
            set
            {
                if (value != null)
                {
                    _cell = value;
                    Coords = (_cell.X, _cell.Z);
                    Renderer.UpdatePosition();
                }
            }
        }

        [JsonIgnore]
        public Color Color
        {
            get
            {
                return ColorHex.GetColorFromHex();
            }
        }

        public Cost Cost { get; set; } = new Cost();

        public string Description { get; set; }

        [JsonIgnore]
        public Faction Faction
        {
            get
            {
                if (_faction == null)
                {
                    _faction = Game.Instance.FactionController.Factions[FactionName];
                }
                return _faction;
            }
        }

        public string FactionName { get; set; }

        public float HP { get; set; } = 5;

        public string Id { get; set; }

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
                if (value != null)
                {
                    InUseById = value.Id;
                }
            }
        }

        [JsonIgnore]
        public bool InUseByAnyone
        {
            get
            {
                return InUseBy != null;
            }
        }

        public string InUseById { get; set; }

        public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

        public string Name { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public StructureRenderer Renderer { get; set; }

        public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

        [JsonIgnore]
        public Vector3 Vector
        {
            get
            {
                return new Vector3(Cell.Vector.x, Game.Instance.MapData.StructureLevel, Cell.Vector.z);
            }
        }

        public bool IsShadowCaster()
        {
            return IsWall();
        }

        public bool IsType(string name)
        {
            return !string.IsNullOrEmpty(Type) && Type.Split(',').Contains(name);
        }

        public bool IsWall()
        {
            return IsType("Wall");
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public bool ValidateCellLocationForStructure(Cell cell)
        {
            // if the cell is empty or full of unbuildable stuff it is acceptable
            if (IsFloor())
            {
                return cell.Floor?.Buildable != true;
            }
            else
            {
                if (cell.TravelCost < 0)
                {
                    return false;
                }
                return cell.Structure?.Buildable != true;
            }
        }

        internal void Free()
        {
            InUseById = null;
        }

        internal string GetProperty(string propertyName)
        {
            if (Properties.ContainsKey(propertyName))
            {
                return Properties[propertyName];
            }
            return string.Empty;
        }

        internal float GetValue(string valueName)
        {
            if (ValueProperties.ContainsKey(valueName))
            {
                return ValueProperties[valueName];
            }
            return 0f;
        }

        internal bool HasValue(string v)
        {
            return ValueProperties.ContainsKey(v);
        }

        internal void HideOutline()
        {
            _outline?.Kill();
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

        internal void SetProperty(string propertyName, string value)
        {
            if (!Properties.ContainsKey(propertyName))
            {
                Properties.Add(propertyName, string.Empty);
            }
            Properties[propertyName] = value;
        }

        internal void SetValue(string valueName, float value)
        {
            if (!ValueProperties.ContainsKey(valueName))
            {
                ValueProperties.Add(valueName, 0f);
            }
            ValueProperties[valueName] = value;
        }

        internal void ShowOutline()
        {
            _outline = Game.Instance.VisualEffectController
                           .SpawnSpriteEffect(this, Vector, "CellOutline", float.MaxValue);
            _outline.Regular();
        }
    }
}