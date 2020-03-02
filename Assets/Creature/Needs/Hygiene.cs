using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Hygiene : NeedBase
    {
        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Dirty", -5, 0, 10),
        };

        public override string Icon { get; set; }

        public override void Update()
        {
        }
    }
}