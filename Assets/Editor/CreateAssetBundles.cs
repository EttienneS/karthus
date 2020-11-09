using UnityEditor;
using System.IO;
using Assets.Map;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Build/AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Build/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
    }

    [CustomEditor(typeof(MapGenerator))]
    public class LevelScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator myTarget = (MapGenerator)target;

            if (GUILayout.Button("Regenerate World"))
            {
                myTarget.Regenerate();
            }
        }
    }
}