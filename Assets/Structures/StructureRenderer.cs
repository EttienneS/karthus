using Assets.Helpers;
using Newtonsoft.Json;
using Structures;
using System;
using System.Net.Sockets;
using UnityEngine;

public class StructureRenderer : MonoBehaviour
{
    internal Structure Data;
    internal MeshRenderer Renderer;

    internal void UpdatePosition()
    {
        transform.position = Data.Vector;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Data.Rotation, transform.eulerAngles.z);
    }

    public void SetBlueprintMaterial()
    {
        Renderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
    }

    internal void UpdateMaterial()
    {
        Debug.Log("Update material");
        if (Data.IsBlueprint)
        {
            Renderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
        }
        else
        {
            Renderer.SetMeshMaterial(Data.DefaultMaterials);
        }
    }
}