using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum EffectType
{
    Light = 1, Sprite = 2, Particle = 4
}

public class VisualEffectController : MonoBehaviour
{
    public Badge BadgePrefab;
    public VisualEffect EffectPrefab;

    private List<Badge> _activeBadges = new List<Badge>();
    private List<VisualEffect> _activeVisualEffects = new List<VisualEffect>();


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
        effect.transform.position = vector;

        return effect;
    }

    public VisualEffect SpawnLightEffect(IEntity holder, Vector2 vector, Color color, float radius, float intensity, float lifeSpan)
    {
        var effect = GetBase(EffectType.Light, holder);

        effect.Light.color = color;
        effect.Data.Intensity = intensity;
        effect.Light.pointLightOuterRadius = radius;
        effect.Data.LifeSpan = lifeSpan;
        effect.transform.position = vector;

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Vector2 vector, string sprite, float lifeSpan)
    {
        var effect = GetBase(EffectType.Sprite, holder);
        effect.Sprite.sprite = Game.SpriteStore.GetSprite(sprite);
        effect.Data.LifeSpan = lifeSpan;
        effect.transform.position = vector;

        return effect;
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