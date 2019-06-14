using System;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    public Material ChannelingMaterial;
    public Material DefaultMaterial;

    public Material GetChannelingMaterial(Color color)
    {
        var material = new Material(ChannelingMaterial);
        material.SetColor("_EffectColor", color);

        if (color == Color.black)
        {
            material.SetFloat("_Balance", 0.5f);
        }

        return material;
    }
}