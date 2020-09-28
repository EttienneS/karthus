using Assets.ServiceLocator;
using UnityEngine;

namespace Assets.Structures
{
    public class WaterBarrel : WorkStructureBase
    {
        public int Capacity { get; set; }
        public int FillLevel { get; set; }

        public override void Update(float delta)
        {
            if (Capacity < 50)
            {

            }
        }
    }
}