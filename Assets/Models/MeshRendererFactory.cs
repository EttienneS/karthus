using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Models
{
    public class MeshRendererFactory : MonoBehaviour
    {
        internal Dictionary<string, MeshRenderer> MeshLookup = new Dictionary<string, MeshRenderer>();

        public void Awake()
        {
            var path = $@"{Environment.CurrentDirectory}\Assets\AssetBundles\structures";
            var bundle = AssetBundle.LoadFromFile(path);

            foreach (var gameObject in bundle.LoadAllAssets<GameObject>())
            {
                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (MeshLookup.ContainsKey(gameObject.name))
                    {
                        Debug.LogError($"Dupe mesh: {gameObject.name}");
                        continue;
                    }
                    MeshLookup.Add(gameObject.name, renderer);
                }
            }
        }

        internal MeshRenderer GetMesh(string name)
        {
            if (MeshLookup.ContainsKey(name))
            {
                return MeshLookup[name];
            }
            return MeshLookup["DefaultCube"];
        }
    }
}