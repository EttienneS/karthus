using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Needs
{
    public class Hygiene : NeedBase
    {
        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Filthy", -15, 0, 10),
            ("Dirty", -5, 0, 25),
        };

        public override float BaselineChangeRate { get; set; } = NeedConstants.BaseDegrateRate;
        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        public override string GetDescription()
        {
            return "The will to do more.";
        }

        public override void Update()
        {
            if (Current < 15 && Creature.IsIdle())
            {
                if (!(Creature.Task is Wash))
                {
                    var bath = Creature
                                .Faction
                                .Structures
                                .Where(s => s.HasValue("Hygiene"))
                                .OrderBy(b => b.Cell.DistanceTo(Creature.Cell))
                                .FirstOrDefault();

                    if (bath != null)
                    {
                        Creature.AbandonTask();
                        Creature.Task = new Wash(bath);
                    }
                    else
                    {
                        Creature.Log("Nowhere to wash!");
                    }
                }
            }
        }
    }
}