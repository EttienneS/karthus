using UnityEngine;

public class EffectController : MonoBehaviour
{
    public Effect EffectPrefab;

    public Badge BadgePrefab;

    public void SpawnEffect(Coordinates coordinates, float lifeSpan)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.LifeSpan = lifeSpan;
        effect.transform.position = coordinates.ToTopOfMapVector();
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

    internal Badge AddBadge(Coordinates coordinates, string iconName)
    {
        var badge = Spawn();
        badge.SetSprite(iconName);
        badge.transform.position = coordinates.ToTopOfMapVector();

        return badge;
    }
}