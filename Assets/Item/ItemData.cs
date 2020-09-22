using Assets.Creature;
using Assets.Map;
using Assets.ServiceLocator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Item
{
    [Serializable]
    public class ItemData 
    {
        private (float X, float Z) _coords;

        private VisualEffect _outline;

        public int Amount { get; set; } = 1;

        public string[] Categories { get; set; }

        [JsonIgnore]
        public Cell Cell
        {
            get
            {
                return Loc.GetMap().GetCellAtCoordinate(Coords.X, Coords.Z);
            }
            set
            {
                Coords = (value.Vector.x, value.Vector.z);
            }
        }

        public (float X, float Z) Coords
        {
            get
            {
                return _coords;
            }
            set
            {
                _coords = value;

                Renderer?.UpdatePosition();
            }
        }

        public Cost Cost { get; set; } = new Cost();
        public string FactionName { get; set; }
        public string Id { get; set; }

        public bool IsReserved()
        {
            return !string.IsNullOrEmpty(InUseById);
        }

        public bool IsStored()
        {
            var storageZone = Loc.GetZoneController().GetZoneForCell(Cell) as StorageZone;
            if (storageZone != null)
            {
                return storageZone.Filter.Allows(this);
            }
            return false;
        }

        public string Icon { get; set; }

        public string InUseById { get; set; }
        public string Mesh { get; set; }
        public string Name { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public ItemRenderer Renderer { get; set; }

        public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

        [JsonIgnore]
        public Vector3 Vector
        {
            get
            {
                return new Vector3(Coords.X, Cell.Y + 0.1f, Coords.Z);
            }
        }

        public static ItemData GetFromJson(string json)
        {
            return json.LoadJson<ItemData>();
        }

        public bool CanUse(CreatureData entity)
        {
            if (string.IsNullOrEmpty(InUseById))
            {
                return true;
            }
            return InUseById.GetCreature() == entity;
        }

        public void Free()
        {
            InUseById = null;
        }

        public void Reserve(CreatureData entity)
        {
            InUseById = entity.Id;
        }

        public override string ToString()
        {
            return $"{Name} ({Amount}) - {Id}";
        }

        internal void HideOutline()
        {
            if (_outline != null)
            {
                _outline.Kill();
            }
        }

        internal bool IsType(string type)
        {
            if (Name.Equals(type, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Categories == null)
                return false;

            return Categories.Contains(type, StringComparer.OrdinalIgnoreCase);
        }

        internal void ShowOutline()
        {
            HideOutline();
            _outline = Loc.GetVisualEffectController()
                           .SpawnSpriteEffect(Vector, "CellOutline", float.MaxValue);
            _outline.Regular();
        }

        internal ItemData Split(int amount)
        {
            Amount -= amount;
            return Loc.GetItemController().SpawnItem(Name, Cell, amount, false);
        }
    }
}