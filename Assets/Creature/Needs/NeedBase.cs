using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public abstract class NeedBase
    {
        [JsonIgnore]
        public abstract List<(string description, int impact, float min, float max)> Levels { get; }

        public float BaselineChangeRate { get; set; } = -0.0025f;

        [JsonIgnore]
        public Creature Creature { get; set; }

        public float Current { get; set; } = 100;
        public float CurrentChangeRate { get; set; } = -0.0025f;
        public abstract string Icon { get; set; }
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

        public override string ToString()
        {
            return $"{Name} [{Current:0.0}/{Max}]";
        }

        public abstract void Update();

        internal void ResetRate()
        {
            CurrentChangeRate = BaselineChangeRate;
        }

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

                try
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
                catch (Exception ex)
                {

                }

            }
        }
    }
}