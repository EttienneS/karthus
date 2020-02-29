using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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


        public void SetMoodFeeling(string feelingName, List<(string description, int impact, float threshold)> levels)
        {
            var feeling = Creature.Feelings.Find(f => f.Name.Equals(feelingName, StringComparison.OrdinalIgnoreCase));

            (string description, int impact, float threshold)? current = null;
            foreach (var level in levels)
            {
                if (Current < level.threshold)
                {
                    current = level;
                    break;
                }
            }

            if (current == null && feeling != null)
            {
                Creature.Feelings.Remove(feeling);
            }
            else
            {
                if (feeling == null)
                {
                    feeling = new Feeling(feelingName, 0, -1f);
                    Creature.Feelings.Add(feeling);
                }

                feeling.Description = current.Value.description;
                feeling.MoodImpact = current.Value.impact;
            }
        }
    }
}