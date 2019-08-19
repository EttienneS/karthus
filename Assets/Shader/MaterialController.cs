using UnityEngine;

public class MaterialController : MonoBehaviour
{
    public Material ChannelingMaterial;
    public Material DefaultMaterial;
    public Material RuneMaterial;
    public Material AbyssMaterial;
    public Material LeyLineMaterial;

    public Material GetMaterial(string name)
    {
        switch (name)
        {
            case "abyss":
                return AbyssMaterial;

            case "leyline":
                return LeyLineMaterial;

            case "rune":
                return RuneMaterial;

            case "channel":
                return ChannelingMaterial;

            default:
                return DefaultMaterial;
        }
    }

    public Material GetLeyLineMaterial(Color color)
    {
        return GetLineMaterial(color, LeyLineMaterial);
    }

    public Material GetChannelingMaterial(Color color)
    {
        return GetLineMaterial(color, ChannelingMaterial);
    }

    public Material GetLineMaterial(Color color, Material baseMat)
    {
        var material = new Material(baseMat);
        material.SetColor("_EffectColor", color);
        return material;
    }
}