using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
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
    public ChannelLine ChannelLinePrefab;
    public VisualEffect EffectPrefab;

    private List<Badge> _activeBadges = new List<Badge>();
    private List<VisualEffect> _activeVisualEffects = new List<VisualEffect>();

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

    public ChannelLine MakeChannellingLine(IEntity source, IEntity target, int intensity, float duration, ManaColor manaColor)
    {
        var line = Instantiate(ChannelLinePrefab, transform);
        line.name = $"Channel: {manaColor}";
        line.SetProperties(source, target, intensity, duration, manaColor);

        return line;
    }

    public Badge Spawn()
    {
        return Instantiate(BadgePrefab, transform);
    }

    public VisualEffect SpawnEffect(VisualEffectData data)
    {
        return GetBase(data.EffectType, data.Holder);
    }

 

    public VisualEffect SpawnEffect(IEntity holder, Vector2 vector, float lifeSpan)
    {
        var effect = GetBase(EffectType.Particle, holder);
        effect.Data.LifeSpan = lifeSpan;
        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());
        return effect;
    }

    public VisualEffect SpawnLightEffect(IEntity holder, Vector2 vector, Color color, float radius, float intensity, float lifeSpan)
    {
        var effect = GetBase(EffectType.Light, holder);

        effect.Light.color = color;
        effect.Data.Intensity = intensity;
        effect.Light.pointLightOuterRadius = radius;
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Vector2 vector, string sprite, float lifeSpan, Color color)
    {
        var effect = GetBase(EffectType.Sprite, holder);

        effect.Data.SetProperty("Sprite", sprite);
        effect.Data.SetProperty("Color", color.ToFloatArrayString());
        effect.Data.LifeSpan = lifeSpan;

        effect.Data.SetProperty("X", vector.x.ToString());
        effect.Data.SetProperty("Y", vector.y.ToString());

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Vector2 vector, string sprite, float lifeSpan)
    {
        return SpawnSpriteEffect(holder, vector, sprite, lifeSpan, Color.white);
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
        badge.transform.position = cell.Vector;

        return badge;
    }
}