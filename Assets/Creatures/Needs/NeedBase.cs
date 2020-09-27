using Assets.Creature;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public abstract class NeedBase
    {
        public abstract float BaselineChangeRate { get; set; }

        [JsonIgnore]
        public CreatureData Creature { get; set; }

        public float Current { get; set; } = 100;

        public float CurrentChangeRate { get; set; }

        public abstract string Icon { get; set; }

        [JsonIgnore]
        public abstract List<(string description, int impact, float min, float max)> Levels { get; }

        public float Max { get; set; } = 100;

        [JsonIgnore]
        public string Name
        {
            get => GetType().Name;
        }

        public void ApplyChange(float delta)
        {
            Current += delta * CurrentChangeRate;
            Current = Mathf.Clamp(Current, 0, Max);

            SetMoodFeeling();
        }

        public abstract string GetDescription();

        public void SetMoodFeeling()
        {
            var feeling = Creature.Feelings.Find(f => f.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));

            (string description, int impact, float min, float max)? current = null;
            foreach (var level in Levels)
            {
                if (Current < level.max && Current > level.min)
                {
                    current = level;
                    break;
                }
            }

            if (!current.HasValue && feeling != null)
            {
                Creature.Feelings.Remove(feeling);
            }
            else
            {
                if (current.HasValue)
                {
                    if (feeling == null)
                    {
                        feeling = new Feeling(Name, 0, -1f);
                        Creature.Feelings.Add(feeling);
                    }

                    feeling.Description = current.Value.description;
                    feeling.MoodImpact = current.Value.impact;
                }
            }
        }

        public override string ToString()
        {
            return $"{Name} [{Current:0}/{Max:0}]";
        }

        public abstract void Update();

        internal void ResetRate()
        {
            CurrentChangeRate = BaselineChangeRate;
        }
    }
}