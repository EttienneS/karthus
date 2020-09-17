using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Comfort : NeedBase
    {
        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Uncomfortable", -10, 0, 20),
            ("Comfortable", 10, 80, 100),
        };

        public override void Update()
        {
            var comfort = Creature.Cell.GetStructureValue("Comfort");
            Current += comfort;
        }

        public override string GetDescription()
        {
            return "The will to do more.";
        }
    }
}