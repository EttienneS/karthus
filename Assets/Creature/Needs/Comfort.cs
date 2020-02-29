using Newtonsoft.Json;
using System.Collections.Generic;

namespace Needs
{
    public class Comfort : NeedBase
    {
        public override string Icon { get; set; }

        

        public override void Update()
        {
            var comfort = Creature.Cell.Structure?.GetValue("Comfort");

            if (comfort.HasValue && comfort != 0)
            {
                Current += comfort.Value;
            }


        }
    }
}