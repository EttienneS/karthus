using Assets.Structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needs
{
    public class Thirst : NeedBase
    {
        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> ThirstLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Dehydrated", -30, 0, 10),
            ("Parched", -15, 10, 30),
            ("Thirsty", 0, 50, 80),
            ("Slaked", 5, 80, 100),
        };

        public override float BaselineChangeRate { get; set; } = NeedConstants.ThirstDegrateRate;

        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => ThirstLevels; }

        public override string GetDescription()
        {
            return "Drinking.";
        }

        public override void Update()
        {
            if (Creature.GetNeed<Thirst>().Current < 50 && Creature.IsIdle())
            {
                if (Creature.HeldItem?.ValueProperties.ContainsKey("Quench") == true)
                {
                    Creature.Task = new Consume(Creature.HeldItem);
                }
                else
                {
                    var drink = Creature.Faction.FindItem("Water", Creature);
                    if (drink != null)
                    {
                        Creature.Task = new Consume(drink);
                    }
                    else
                    {
                        Creature.Task = new FindAndGetItem("Water",1);
                    }
                }
            }
        }
    }
}