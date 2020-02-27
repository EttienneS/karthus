using System;

namespace Needs
{
    public class Hunger : NeedBase
    {
        public override string Icon { get; set; }

        public const string FeelingName = "Hunger";

        public override void Update()
        {
            var feeling = Creature.Feelings.Find(f => f.Name.Equals(FeelingName, StringComparison.OrdinalIgnoreCase));

            if (Current < 70 && Current > 30)
            {
                if (feeling != null)
                {
                    Creature.Feelings.Remove(feeling);
                }
            }
            else
            {
                if (feeling == null)
                {
                    feeling = new Feeling
                    {
                        Name = FeelingName
                    };
                    Creature.Feelings.Add(feeling);
                }

                if (Current > 70)
                {
                    feeling.Description = "Full";
                    feeling.MoodImpact = 10;
                }
                else if (Current < 30)
                {
                    feeling.Description = "Hungry";
                    feeling.MoodImpact = -10;
                }
            }
        }
    }
}