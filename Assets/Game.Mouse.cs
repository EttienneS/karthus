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

    internal float MouseOffset = 0;

    public void SetMouseSprite(Sprite sprite, int width, int height, bool tiled, ValidateMouseSpriteDelegate validation, float offset = 0)
    {
        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = sprite;
        MouseSpriteRenderer.transform.localScale = new Vector3(width, height, 1);
        MouseSpriteRenderer.drawMode = tiled ? SpriteDrawMode.Tiled : SpriteDrawMode.Simple;

        MouseOffset = offset;

        ValidateMouse = validation;


    }

    private void MoveMouseSprite(Vector3 mousePosition)
    {
        if (MouseSpriteRenderer.gameObject.activeInHierarchy)
        {
            var cell = MapGrid.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));

            float x = cell.Coordinates.X + (MouseSpriteRenderer.transform.localScale.x / 2) + MouseOffset;
            float y = cell.Coordinates.Y + (MouseSpriteRenderer.transform.localScale.y / 2) + MouseOffset;

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