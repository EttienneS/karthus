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
                if (Creature.HeldItem?.Name == "Water")
                {
                    Creature.Task = new Consume(Creature.HeldItem);
                }
                else
                {
                    var drink = Creature.Faction.FindItem("Drink", Creature);
                    if (drink != null)
                    {
                        Creature.Task = new Consume(drink);
                    }
                    else
                    {
                        var containers = Creature.Faction.GetClosestStructures<LiquidContainer>(Creature.Cell)
                                                         .Where(l => l.FillLevel > 0)
                                                         .ToList();

                        if (containers.Count > 0)
                        {
                            var container = containers.First();
                            Creature.Task = new GetWaterFromContainer(container);
                            Debug.Log($"Getting water from {container.Name}");
                        }
                        else
                        {
                            Creature.Task = new GetWaterFromSource();
                            Debug.LogWarning("Nothing to drink, get water from a source!");
                        }

                        
                    }
                }
            }
        }
    }
}