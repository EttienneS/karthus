using Assets.Helpers;
using Assets.ServiceLocator;
using UnityEngine;

namespace Assets.Structures
{
    public class BlueprintRenderer : MonoBehaviour
    {
        public Blueprint Blueprint;

        public void Load(Blueprint blueprint)
        {
            Blueprint = blueprint;
            Blueprint.BlueprintRenderer = this;
            transform.position = new Vector3(Blueprint.Cell.Vector.x, Blueprint.Cell.Vector.y, Blueprint.Cell.Vector.z);
        }

        internal void SetSuspendedMaterial()
        {
            GetComponent<MeshRenderer>().SetAllMaterial(Loc.GetFileController().SuspendedBlueprintMaterial);
        }

        internal void SetDefaultMaterial()
        {
            GetComponent<MeshRenderer>().SetAllMaterial(Loc.GetFileController().BlueprintMaterial);
        }
    }
}