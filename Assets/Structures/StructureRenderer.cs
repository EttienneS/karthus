using Newtonsoft.Json;
using Structures;
using System.Net.Sockets;
using UnityEngine;

public class StructureRenderer : MonoBehaviour
{
    internal Structure Data;

    [JsonIgnore]
    public SpriteRenderer MeshRenderer;

    internal void UpdatePosition()
    {
        transform.position = Data.Vector;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Data.Rotation, transform.eulerAngles.z);
    }
}