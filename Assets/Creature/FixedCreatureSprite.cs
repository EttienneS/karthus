using UnityEngine;

public class FixedCreatureSprite : ICreatureSprite
{
    public SpriteRenderer Sprite;

    public string SpriteName;

    public FixedCreatureSprite(string spriteName, Creature creature)
    {
        SpriteName = spriteName;

        Sprite = GameObject.Instantiate(creature.BodyPartPrefab, creature.Body.transform);
        creature.Body.transform.localScale = Vector3.one;
        Sprite.sprite = Game.SpriteStore.GetFixedCreatureSprite(spriteName);

        Sprite.name = spriteName;
    }

    public Color CurrentColor { get; set; }

    public Sprite GetIcon()
    {
        return Sprite.sprite;
    }
    public void Update(Color color)
    {
        if (color != CurrentColor)
        {
            CurrentColor = color;
            Sprite.color = CurrentColor;
        }
    }
}