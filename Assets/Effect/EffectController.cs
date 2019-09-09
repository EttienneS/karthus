using UnityEngine;

public class EffectController : MonoBehaviour
{
    public Effect EffectPrefab;

    public Badge BadgePrefab;

    public void SpawnEffect(Cell cell, float lifeSpan)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();
    }

    public Badge Spawn()
    {
        return Instantiate(BadgePrefab, transform);
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