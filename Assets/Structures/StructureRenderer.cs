using Assets.Map;
using Assets.ServiceLocator;
using UnityEngine;

namespace Assets.Structures
{
    public class StructureRenderer : MonoBehaviour
    {
        public Structure Data;
        internal MeshRenderer Renderer;

        internal void UpdatePosition(ChunkRenderer chunk)
        {
            var pos = Data.GetVector();
            pos -= new Vector3(chunk.Data.X * MapGenerationData.Instance.ChunkSize, 0, chunk.Data.Z * MapGenerationData.Instance.ChunkSize);
            pos -= new Vector3(0.5f, 0, 0.5f);

            transform.localPosition = pos;
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

        public void Update()
        {
            if (!Loc.GetGameController().Paused)
            {
                Data.Update(Time.deltaTime);
            }
        }
    }
}