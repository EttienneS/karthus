using System;
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

    public VisualEffect SpawnEffect(IEntity holder, Cell cell, float lifeSpan)
    {
        var effect = GetBase(EffectType.Particle, holder);
        effect.Data.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();

        return effect;
    }

    public VisualEffect SpawnLightEffect(IEntity holder, Cell cell, Color color, float radius, float intensity, float lifeSpan)
    {
        var effect = GetBase(EffectType.Light, holder);
        
        effect.Light.color = color;
        effect.Data.Intensity = intensity;
        effect.Light.pointLightOuterRadius = radius;
        effect.Data.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();

        return effect;
    }

    public VisualEffect SpawnSpriteEffect(IEntity holder, Cell cell, string sprite, float lifeSpan)
    {
        var effect = GetBase(EffectType.Sprite, holder);
        effect.Sprite.sprite = Game.SpriteStore.GetSprite(sprite);
        effect.Data.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();

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
        badge.transform.position = cell.ToTopOfMapVector();

        return badge;
    }
}