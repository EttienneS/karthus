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

public class VisualEffectController : MonoBehaviour
{
    public Badge BadgePrefab;
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

    public VisualEffect Load(VisualEffectData data)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.Data = data;
        return effect;
    }

    public Badge Spawn()
    {
        return Instantiate(BadgePrefab, transform);
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

    internal void CreateFireLight(GameObject lightObject, Color color, float range, float intensity)
    {
        var light = lightObject.AddComponent<Light>();
        light.color = color;
        light.range = range;
        light.intensity = intensity;
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

    public void Update()
    {
        //foreach (var badge in _activeBadges)
        //{
        //    if (badge.Entity == null)
        //    {
        //        Destroy(badge.gameObject);
        //    }
        //}

        //foreach (var visualEffect in _activeVisualEffects)
        //{
        //    if (visualEffect.Entity == null)
        //    {
        //        Destroy(visualEffect.gameObject);
        //    }
        //}
    }

    internal Badge AddBadge(IEntity entity, string iconName)
    {
        var badge = Spawn();
        badge.SetSprite(iconName);
        badge.Follow(entity);
        return badge;
    }

    internal Badge AddBadge(Cell cell, string iconName)
    {
        var badge = Spawn();
        badge.SetSprite(iconName);
        badge.transform.position = cell.Vector + new Vector3(0, 0.2f, 0);

        return badge;
    }
}