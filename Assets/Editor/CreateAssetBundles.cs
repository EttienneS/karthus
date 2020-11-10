using UnityEditor;
using System.IO;

public partial class CreateAssetBundles
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
}