﻿using Newtonsoft.Json;
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

    public VisualEffect GetBase(EffectType effectType, IEntity holder)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.Data = new VisualEffectData
        {
            EffectType = effectType
        };

        if (holder != null)
        {
            effect.Data.HolderId = holder.Id;
            holder.LinkedVisualEffects.Add(effect.Data);
        }
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
        return GetBase(data.EffectType, data.Holder);
    }

    public VisualEffect SpawnEffect(IEntity holder, Vector3 vector, float lifeSpan)
    {
        var effect = GetBase(EffectType.Particle, holder);
        effect.Data.LifeSpan = lifeSpan;
        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());
        return effect;
    }

    public VisualEffect SpawnLightEffect(IEntity holder, Vector3 vector, Color color, float radius, float intensity, float lifeSpan)
    {
        var effect = GetBase(EffectType.Light, holder);

        effect.Light.color = color;
        effect.Data.Intensity = intensity;
        //effect.Light.pointLightOuterRadius = radius;
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Vector3 vector, string sprite, float lifeSpan, Color color)
    {
        var effect = GetBase(EffectType.Sprite, holder);

        effect.Data.SetProperty("Sprite", sprite);
        effect.Data.SetProperty("Color", color.ToColorHexString());
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        effect.Data.SetProperty("Z", vector.z.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Vector3 vector, string sprite, float lifeSpan)
    {
        return SpawnSpriteEffect(holder, vector, sprite, lifeSpan, ColorConstants.WhiteBase);
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