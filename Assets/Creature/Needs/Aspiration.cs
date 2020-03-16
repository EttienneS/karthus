using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Aspiration : NeedBase
    {
        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => AspirationLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> AspirationLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Uninspired", -5, 0, 10),
        };

        public override void Update()
        {
        }

        public override string GetDescription()
        {
            return "The will to do more.";
        }
    }
}