using UnityEngine;

public interface ICreatureSprite
{
    void Update(Color color);

    Sprite GetIcon();

    Color CurrentColor { get; set; }
}