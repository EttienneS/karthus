using UnityEngine;

public partial class Game // .Mouse
{
    public SpriteRenderer MouseSpriteRenderer;

    public ValidateMouseSpriteDelegate ValidateMouse;

    public delegate bool ValidateMouseSpriteDelegate(CellData currentCell);

    public void DisableMouseSprite()
    {
        MouseSpriteRenderer.gameObject.SetActive(false);
        ValidateMouse = null;
    }
    public void SetMouseSprite(Sprite sprite, int width, int height, bool tiled, ValidateMouseSpriteDelegate validation)
    {
        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = sprite;
        MouseSpriteRenderer.drawMode = tiled ? SpriteDrawMode.Tiled : SpriteDrawMode.Simple;
        MouseSpriteRenderer.transform.localScale = new Vector3(width, height, 1);

        ValidateMouse = validation;
    }

    private void MoveMouseSprite(Vector3 mousePosition)
    {
        if (MouseSpriteRenderer.gameObject.activeInHierarchy)
        {
            var cell = global::Game.MapGrid.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));

            float x = cell.Coordinates.X + (MouseSpriteRenderer.transform.localScale.x / 2);
            float y = cell.Coordinates.Y + (MouseSpriteRenderer.transform.localScale.y / 2);

            MouseSpriteRenderer.transform.position = new Vector2(x, y);

            if (ValidateMouse != null)
            {
                if (!ValidateMouse(cell))
                {
                    MouseSpriteRenderer.color = Color.red;
                }
                else
                {
                    MouseSpriteRenderer.color = Color.white;
                }
            }
        }
    }
}