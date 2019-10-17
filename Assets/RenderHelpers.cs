using UnityEngine;

public static class RenderHelpers
{
    public static void SetBoundMaterial(this SpriteRenderer renderer, Cell cell)
    {
        if (cell.Bound)
        {
            renderer.material = Game.MaterialController.DefaultMaterial;
        }
        else
        {
            renderer.material = Game.MaterialController.AbyssMaterial;
        }

        renderer.color = cell.Color;
    }
}