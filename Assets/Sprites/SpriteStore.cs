using Assets.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteStore : MonoBehaviour, IGameService
{
    internal Dictionary<string, Sprite> IconSprites { get; set; }
    internal Dictionary<string, Material> SkinMaterials { get; set; }

    public void Initialize()
    {
        LoadIcons();
        LoadSkins();
    }

    private void LoadSkins()
    {
        SkinMaterials = new Dictionary<string, Material>();

        var materials = Resources.LoadAll<Material>("Skins").ToList();
        foreach (var material in materials)
        {
            SkinMaterials.Add(material.name, material);
        }
    }

    private void LoadIcons()
    {
        IconSprites = new Dictionary<string, Sprite>();

        var sprites = Resources.LoadAll<Sprite>("Sprites/Icons").ToList();
        sprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Gui"));

        foreach (var sprite in sprites)
        {
            IconSprites.Add(sprite.name, sprite);
        }
    }

    internal bool FacingUp(Direction facing)
    {
        switch (facing)
        {
            case Direction.NW:
            case Direction.NE:
            case Direction.N:
                return true;

            default:
                return false;
        }
    }

    internal Sprite GetPlaceholder()
    {
        return GetSprite("Placeholder");
    }

    internal Sprite GetSprite(string spriteName)
    {
        try
        {
            if (!IconSprites.ContainsKey(spriteName))
            {
                spriteName = spriteName.Replace(" ", "");
            }

            if (IconSprites.ContainsKey(spriteName))
            {
                return IconSprites[spriteName];
            }


            Debug.LogWarning($"No sprite for: {spriteName}");
            return GetPlaceholder();
        }
        catch
        {
            throw new Exception($"No sprite found with name: {spriteName}");
        }
    }

    public Material GetRandomSkin()
    {
        return SkinMaterials.Values.GetRandomItem();
    }

    public Material GetSkin(string skinName)
    {
        if (SkinMaterials.ContainsKey(skinName))
        {
            return SkinMaterials[skinName];
        }
        throw new Exception($"No skin found with name: {skinName}");
    }
}