using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Helpers
{
    public static class MeshHelper
    {
        public static void SetMeshMaterial(this MeshRenderer meshRenderer, params Material[] materials)
        {
            meshRenderer.materials = materials;
        }

        public static void SetAllMaterial(this MeshRenderer meshRenderer, Material material)
        {
            var mats = new List<Material>();
            for (var i = 0; i < meshRenderer.materials.Length; i++)
            {
                mats.Add(material);
            }

            meshRenderer.SetMeshMaterial(mats.ToArray());
        }
    }
}
