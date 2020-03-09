using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Structures;

namespace Needs
{
    public class Hygiene : NeedBase
    {
        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Filthy", -15, 0, 10),
            ("Dirty", -5, 0, 25),
        };

        public override string Icon { get; set; }

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
                                .OrderBy(b => Pathfinder.Distance(b.Cell, Creature.Cell, Creature.Mobility))
                                .FirstOrDefault();

                    if (bath != null)
                    {
                        Creature.CancelTask();
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