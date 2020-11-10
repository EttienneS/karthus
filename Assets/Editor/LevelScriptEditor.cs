using UnityEditor;
using Assets.Map;
using UnityEngine;

public partial class CreateAssetBundles
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var myTarget = (MapGenerator)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Regenerate World"))
            {
                myTarget.Regenerate();
            }
        }
    }
}