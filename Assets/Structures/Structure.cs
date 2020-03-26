using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Structures
{
    public class Structure : IEntity
    {
        public bool Buildable;

        [JsonIgnore]
        public VisualEffect ContainedItemEffect;

        // rather than serializing the cell object we keep this lazy link for load
        public (int X, int Y) Coords = (-1, -1);

        public string Layer;

        public Direction Rotation;

        public string Size;

        public string SpriteName;

        public float TravelCost;

        public string Type;

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
                    Renderer.UpdatePosition();
                }
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

        public bool IsBluePrint { get; set; }

        public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

        public string Name { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public StructureRenderer Renderer { get; set; }

        public Sprite GetSprite()
        {
            Sprite sprite;
            if (IsWall())
            {
                sprite = Game.SpriteStore.GetInterlockingSprite(this);
            }
            else
            {
                sprite = Game.SpriteStore.GetSprite(SpriteName);
            }

            if (IsBluePrint)
            {
                Renderer.SpriteRenderer.color = ColorConstants.BluePrintColor;
            }
            else
            {
                Renderer.SpriteRenderer.color = Cell.Color;
            }

            return sprite;
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

        public bool IsType(string name)
        {
            return !string.IsNullOrEmpty(Type) && Type.Split(',').Contains(name);
        }

        public bool IsWall()
        {
            return IsType("Wall");
        }

        public bool IsShadowCaster()
        {
            return IsWall();
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

        public override string ToString()
        {
            return $"{Name}";
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
    }
}