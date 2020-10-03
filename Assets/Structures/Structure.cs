using Assets.Creature;
using Assets.Map;
using Assets.ServiceLocator;
using Assets.Structures.Behaviour;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Structures
{
    [Serializable]
    public class Structure
    {
        public float BaseFlammability = -1f;
        public bool Buildable;

        public string ColorHex = "#ffffff";

        [JsonIgnore]
        public VisualEffect ContainedItemEffect;

        // rather than serializing the cell object we keep this lazy link for load
        public (int X, int Y) Coords = (-1, -1);

        public float Flammability;
        public string Icon;
        public string Layer;

        public string Mesh;
        public float Rotation;

        public bool SpawnRotation;
        public List<StructureBehaviour> StructureBehaviours = new List<StructureBehaviour>();
        public float TravelCost;

        public string Type;
        private Cell _cell;

        private Faction _faction;

        private VisualEffect _outline;

        public Structure()
        {
        }

        public Structure(string name, string mesh) : this()
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
                    _cell = Loc.GetMap().GetCellAtCoordinate(Coords.X, Coords.Y);
                }
                return _cell;
            }
            set
            {
                if (value != null)
                {
                    _cell = value;
                    Coords = (_cell.X, _cell.Z);
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
                    _faction = Loc.GetFactionController().Factions[FactionName];
                }
                return _faction;
            }
        }

        public string FactionName { get; set; }

        public float HP { get; set; } = 5;

        public string Id { get; set; }

        [JsonIgnore]
        public CreatureData InUseBy
        {
            get
            {
                if (string.IsNullOrEmpty(InUseById))
                {
                    return null;
                }
                return InUseById.GetCreature();
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

        public string Name { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public StructureRenderer Renderer { get; set; }

        public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

        public Vector3 GetVector()
        {

            return new Vector3(Cell.Vector.x, Cell.Vector.y, Cell.Vector.z);
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

        public virtual void Load()
        {
            Flammability = BaseFlammability;
            if (SpawnRotation)
            {
                Rotation = UnityEngine.Random.Range(1, 360);
            }
        }

        public virtual void OnDestroy()
        {

        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public void Update(float delta)
        {
            foreach (var behaviour in StructureBehaviours.ToList())
            {
                behaviour.Update(delta);
            }
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
                return cell.Structures.All(s => !s.Buildable);
            }
        }

        internal void AddBehaviour<T>() where T : StructureBehaviour
        {
            var behaviour = (T)Activator.CreateInstance(typeof(T));
            StructureBehaviours.Add(behaviour);
            behaviour.Link(this);
        }

        internal bool Flammable()
        {
            return Flammability > 0 && !HasBehaviour<Wildfire>();
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

        internal bool HasBehaviour<T>() where T : StructureBehaviour
        {
            return StructureBehaviours.Any(s => s is T);
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

        internal void Reserve(CreatureData reservedBy)
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
            HideOutline();
            _outline = Loc.GetVisualEffectController()
                           .SpawnSpriteEffect(GetVector(), "CellOutline", float.MaxValue);
            _outline.Regular();
        }
    }
}