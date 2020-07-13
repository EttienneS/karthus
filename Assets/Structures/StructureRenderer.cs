using UnityEngine;

namespace Assets.Structures
{
    public class StructureRenderer : MonoBehaviour
    {
        internal Structure Data;
        internal MeshRenderer Renderer;

        internal void UpdatePosition()
        {
            transform.position = Data.Vector;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Data.Rotation, transform.eulerAngles.z);
        }
    }
}