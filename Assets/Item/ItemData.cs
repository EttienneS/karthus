using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Item
{
    public class ItemData : IEntity
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
                return Map.Instance.GetCellAtCoordinate(Coords);
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
        [JsonIgnore]
        public bool InUseByAnyone
        {
            get
            {
                return !string.IsNullOrEmpty(InUseById);
            }
        }

        public string Icon { get; set; }

        public string InUseById { get; set; }
        public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();
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

        

        public bool CanUse(IEntity entity)
        {
            if (string.IsNullOrEmpty(InUseById))
            {
                return true;
            }
            return InUseById.GetEntity() == entity;
        }

        public void Free()
        {
            InUseById = null;
        }

        public void Reserve(IEntity entity)
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
            _outline = Game.Instance.VisualEffectController
                           .SpawnSpriteEffect(this, Vector, "CellOutline", float.MaxValue);
            _outline.Regular();
        }

        internal ItemData Split(int amount)
        {
            Amount -= amount;
            return Game.Instance.ItemController.SpawnItem(Name, Cell, amount);
        }
    }
}