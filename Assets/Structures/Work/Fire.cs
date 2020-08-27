using UnityEngine;

namespace Structures.Work
{
    public class Fire : WorkStructureBase
    {
        private MeshRenderer _flameMesh;

        public float Radius { get; set; } = 3f;
        public float Intensity { get; set; } = 1f;

        public override void Update(float delta)
        {
            if (_flameMesh == null)
            {
                _flameMesh = Game.Instance.MeshRendererFactory
                                          .InstantiateMesh(Game.Instance.MeshRendererFactory.GetStructureMesh("Flames"),
                                                           Renderer.transform);
                _flameMesh.transform.localPosition += new Vector3(0, -0.2f, 0);

                CreateLight();
            }
        }

        private void CreateLight()
        {
            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(_flameMesh.transform);
            lightObject.transform.localPosition = new Vector3(0, 1f, 0);

            var light = lightObject.AddComponent<Light>();
            light.color = ColorHex.GetColorFromHex();
            light.range = Radius;
            light.intensity = Intensity;
        }
    }
}