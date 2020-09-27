using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class Hunger : NeedBase
    {
        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> HungerLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Ravenous",-20, 0, 10),
            ("Hungry",-10, 10, 30),
            ("Full", 5, 90, 100),
        };

        public override float BaselineChangeRate { get; set; } = NeedConstants.BaseDegrateRate;
        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => HungerLevels; }

        public override string GetDescription()
        {
            return "The will to do more.";
        }

        public override void Update()
        {
            if (Creature.GetNeed<Hunger>().Current < 50 && Creature.IsIdle())
            {
                var food = Creature.Faction.FindItem("Food", Creature);
                if (food != null)
                {
                    Creature.Task = new Consume(food);
                }
                else
                {
                    Debug.LogWarning("No food items found!");
                }
            }
        }
    }
}