using UnityEngine;

public interface ICreatureSprite
{
    void Update(Color color);

    Sprite GetIcon();

    void SetBodyMaterial(Material material);
}