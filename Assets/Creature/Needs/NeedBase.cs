using System;
using UnityEngine;

namespace Needs
{
    public abstract class NeedBase
    {
        public abstract string Icon { get; set; }
        public float BaselineChangeRate { get; set; } = -0.025f;
        public float CurrentChangeRate { get; set; } = -0.025f;
        public float Current { get; set; } = 100;
        public float Max { get; set; } = 100;

        public void ApplyChange()
        {
            Current += CurrentChangeRate;
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