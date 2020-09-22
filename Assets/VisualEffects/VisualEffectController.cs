using Assets.ServiceLocator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using TMPro;
using UnityEngine;

[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum EffectType
{
    Light = 1, Sprite = 2, Particle = 4
}

public class VisualEffectController : MonoBehaviour, IGameService
{
    public VisualEffect EffectPrefab;
    public TextMeshPro TextMeshPrefab;

    public VisualEffect GetBase(EffectType effectType)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.Data = new VisualEffectData
        {
            EffectType = effectType
        };

        return effect;
    }

    public void Initialize()
    {
    }

    public VisualEffect Load(VisualEffectData data)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.Data = data;
        return effect;
    }

    public VisualEffect SpawnEffect(VisualEffectData data)
    {
        return GetBase(data.EffectType);
    }

    public VisualEffect SpawnEffect(Vector3 vector, float lifeSpan)
    {
        var effect = GetBase(EffectType.Particle);
        effect.Data.LifeSpan = lifeSpan;
        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());
        return effect;
    }

    public VisualEffect SpawnLightEffect(Vector3 vector, Color color, float radius, float intensity, float lifeSpan)
    {
        var effect = GetBase(EffectType.Light);

        effect.Light.color = color;
        effect.Data.Intensity = intensity;
        //effect.Light.pointLightOuterRadius = radius;
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(Vector3 vector, string sprite, float lifeSpan, Color color)
    {
        var effect = GetBase(EffectType.Sprite);

        effect.Data.SetProperty("Sprite", sprite);
        effect.Data.SetProperty("Color", color.ToColorHexString());
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(Vector3 vector, string sprite, float lifeSpan)
    {
        return SpawnSpriteEffect(vector, sprite, lifeSpan, ColorConstants.WhiteBase);
    }

    internal TextMeshPro AddTextPrefab(GameObject gameObject)
    {
        return Instantiate(TextMeshPrefab, gameObject.transform);
    }

    internal void CreateFireLight(Transform parent, Color color, float range, float intensity, float height)
    {
        var lightObject = new GameObject("Fire Light");
        lightObject.transform.SetParent(parent);
        lightObject.transform.localPosition = new Vector3(0, height, 0);

        var light = lightObject.AddComponent<Light>();
        light.color = color;
        light.range = range;
        light.intensity = intensity;
    }
}