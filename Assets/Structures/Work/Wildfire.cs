using System.Linq;
using UnityEngine;

namespace Structures.Work
{
    public class Wildfire : WorkStructureBase
    {
        private bool _runOnce;
        private float _spreadDelta;

        private int _spreadCount;

        public override void Update(float delta)
        {
            if (!_runOnce)
            {
                _runOnce = true;
                var scale = Random.value + 0.5f;
                Renderer.transform.localScale += new Vector3(scale, scale, scale);
            }

            if (_spreadCount > 4)
            {
                return;
            }
            _spreadDelta += delta;

            if (_spreadDelta > 25)
            {
                _spreadCount++;
                _spreadDelta = 0;

                var open = Map.Instance.GetCircle(Cell, 3).Where(c => c.Structure != null && c.Structure.Name != "Fire")
                                       .OrderBy(c => c.DistanceTo(Cell)).ToList();

                if (open.Count > 0)
                {
                    var spreadStructure = open.GetRandomItem().Structure;
                    var meshName = spreadStructure.Mesh;

                    var fireStructure = Game.Instance.StructureController
                                                     .SpawnStructure("Fire",
                                                                     spreadStructure.Cell,
                                                                     Game.Instance.FactionController.WorldFaction);

                    Game.Instance.StructureController.DestroyStructure(spreadStructure);
                }
            }
        }
    }
}