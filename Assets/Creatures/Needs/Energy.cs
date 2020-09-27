using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Energy : NeedBase
    {
        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> EnergyLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Tired",-10, 0, 10),
            ("Rested", 10, 80, 100),
        };

        public override float BaselineChangeRate { get; set; } = NeedConstants.BaseDegrateRate;
        public override string Icon { get; set; }

        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => EnergyLevels; }

        public override string GetDescription()
        {
            return "The will to do more.";
        }

        public override void Update()
        {
            if (Creature.Task is Sleep)
            {
                if (Creature.Cell.HasStructureValue("RecoveryRate"))
                {
                    CurrentChangeRate = Creature.Cell.GetStructureValue("RecoveryRate");
                }
                else
                {
                    CurrentChangeRate = BaselineChangeRate;
                }
            }
            else
            {
                if (Creature.GetNeed<Energy>().Current < 15)
                {
                    Creature.AbandonTask();
                    Creature.Task = new Sleep();
                }
            }
        }
    }
}