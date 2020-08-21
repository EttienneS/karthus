using UnityEngine;

namespace Assets.Structures
{
    public class StructureRenderer : MonoBehaviour
    {
        public Structure Data;
        internal MeshRenderer Renderer;

        internal void UpdatePosition()
        {
            transform.position = Data.Vector;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Data.Rotation, transform.eulerAngles.z);
        }

        internal MeshRenderer InstantiateSubMesh(MeshRenderer mesh)
        {
            return Instantiate(mesh, transform);
        }

        internal void DestroySubMesh(MeshRenderer plantMesh)
        {
            Destroy(plantMesh);
        }
    }
}