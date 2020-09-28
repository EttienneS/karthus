using Assets.ServiceLocator;
using UnityEngine;

namespace Assets.Structures
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

        internal static MeshRenderer CreateFlameMeshWithLight(float range, float intensity, Color color, Transform parent)
        {
            var mesh = Loc.GetGameController().MeshRendererFactory.CreateFlameMesh(parent);
            mesh.transform.localPosition += new Vector3(0, -0.2f, 0);

            Loc.GetVisualEffectController().CreateFireLight(mesh.transform, color, range, intensity, 1f);

            return mesh;
        }
    }
}