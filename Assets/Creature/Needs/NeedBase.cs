using Newtonsoft.Json;
using UnityEngine;

namespace Needs
{
    public abstract class NeedBase
    {
        public float BaselineChangeRate { get; set; } = -0.025f;

        [JsonIgnore]
        public Creature Creature { get; set; }

        public float Current { get; set; } = 100;
        public float CurrentChangeRate { get; set; } = -0.025f;
        public abstract string Icon { get; set; }
        public float Max { get; set; } = 100;

        public void ApplyChange(float delta)
        {
            Current += delta * CurrentChangeRate;
            Current = Mathf.Clamp(Current, 0, Max);
        }

        public override string ToString()
        {
            return $"{GetType().Name} [{Current:0.0}/{Max}]";
        }

        public abstract void Update();

        internal void ResetRate()
        {
            CurrentChangeRate = BaselineChangeRate;
        }
    }
}