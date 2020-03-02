using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Energy : NeedBase
    {
        [JsonIgnore]
        public override List<(string description, int impact, float min, float max)> Levels { get => EnergyLevels; }

        [JsonIgnore]
        public static List<(string description, int impact, float min, float max)> EnergyLevels = new List<(string description, int impact, float min, float max)>
        {
            ("Tired",-10, 0, 10),
            ("Rested", 10, 80, 100),
        };

        public override string Icon { get; set; }

        public override void Update()
        {
            if (Current <= 0)
            {
                Creature.CancelTask();

                // collapse and sleep where you are
                Creature.Task = new Sleep();
                Creature.CreatureRenderer.ShowText($"*{Creature.Name} passes out from exhaustion.*", 10);
            }

            if (Creature.Task is Sleep)
            {
                if (Creature.Cell.Structure?.HasValue("RecoveryRate") == true)
                {
                    CurrentChangeRate = Creature.Cell.Structure.GetValue("RecoveryRate");
                }
                else
                {
                    CurrentChangeRate = BaselineChangeRate;
                }
            }
        }
    }
}