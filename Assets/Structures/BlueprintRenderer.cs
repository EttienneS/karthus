using Assets.Helpers;
using System;
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
            transform.position = new Vector3(Blueprint.Cell.Vector.x, Game.Instance.MapData.StructureLevel, Blueprint.Cell.Vector.z);
        }

        internal void SetSuspendedMaterial()
        {
            GetComponent<MeshRenderer>().SetAllMaterial(Game.Instance.FileController.SuspendedBlueprintMaterial);
        }

        internal void SetDefaultMaterial()
        {
            GetComponent<MeshRenderer>().SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
        }


    }
}