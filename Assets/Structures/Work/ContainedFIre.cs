using Assets.Structures;
using UnityEngine;

namespace Structures.Work
{

    public class ContainedFire : WorkStructureBase
    {
        private MeshRenderer _flameMesh;

        public float Radius { get; set; } = 3f;
        public float Intensity { get; set; } = 1f;

        public override void Update(float delta)
        {
            if (_flameMesh == null)
            {
                _flameMesh = CreateFlameMeshWithLight(Radius, Intensity, ColorHex.GetColorFromHex(), Renderer.transform);
            }
        }

        internal MeshRenderer CreateFlameMeshWithLight(float range, float intensity, Color color, Transform parent)
        {
            var mesh = Game.Instance.MeshRendererFactory
                                          .InstantiateMesh(Game.Instance.MeshRendererFactory.GetStructureMesh("Flames"),
                                                           parent);
            mesh.transform.localPosition += new Vector3(0, -0.2f, 0);

            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(mesh.transform);
            lightObject.transform.localPosition = new Vector3(0, 1f, 0);

            Game.Instance.VisualEffectController.CreateFireLight(lightObject, color, range, intensity);

            return mesh;
        }

    }
}