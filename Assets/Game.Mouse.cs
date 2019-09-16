using UnityEngine;

public partial class Game // .Mouse
{
    public SpriteRenderer MouseSpriteRenderer;

    public ValidateMouseSpriteDelegate ValidateMouse;
    public Rotate RotateMouseRight;
    public Rotate RotateMouseLeft;

    public delegate void Rotate();

    public delegate bool ValidateMouseSpriteDelegate(Cell currentCell);

    public void DisableMouseSprite()
    {
        MouseSpriteRenderer.gameObject.SetActive(false);
        ValidateMouse = null;
        RotateMouseRight = null;
    }

    public void SetMouseSprite(string spriteName, ValidateMouseSpriteDelegate validation)
    {
        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = SpriteStore.GetSprite(spriteName);
        ValidateMouse = validation;
    }

    private void MoveMouseSprite(Vector3 mousePosition)
    {
        if (MouseSpriteRenderer.gameObject.activeInHierarchy)
        {
            var cell = Map.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));
            float x = cell.X + 0.5f;
            float y = cell.Y + 0.5f;

            MouseSpriteRenderer.transform.position = new Vector2(x, y);

            if (ValidateMouse != null)
            {
                if (!ValidateMouse(cell))
                {
                    MouseSpriteRenderer.color = ColorConstants.InvalidColor;
                }
                else
                {
                    MouseSpriteRenderer.color = ColorConstants.BluePrintColor;
                }
            }
        }
    }
}