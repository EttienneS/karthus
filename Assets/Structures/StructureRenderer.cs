using Newtonsoft.Json;
using Structures;
using UnityEngine;

public class StructureRenderer : MonoBehaviour
{
    internal Structure Data;

    [JsonIgnore]
    public SpriteRenderer MeshRenderer;

    internal void UpdatePosition()
    {
        transform.position = Data.Vector;
    }
}