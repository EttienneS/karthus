using UnityEngine;

public partial class Game // .Mouse
{
    public SpriteRenderer MouseSpriteRenderer;

    public ValidateMouseSpriteDelegate ValidateMouse;
    public Rotate RotateMouseRight;
    public Rotate RotateMouseLeft;

    public delegate void Rotate();

    public delegate bool ValidateMouseSpriteDelegate(CellData currentCell);

    public void DisableMouseSprite()
    {
        MouseSpriteRenderer.gameObject.SetActive(false);
        ValidateMouse = null;
        RotateMouseRight = null;
    }

    public void SetMouseSprite(Texture2D texture, int width, int height, ValidateMouseSpriteDelegate validation)
    {
        var mouseTex = texture.Clone();
        mouseTex.ScaleToGridSize(width, height);

        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = Sprite.Create(mouseTex,
                                                   new Rect(0, 0, width * MapGrid.PixelsPerCell, height * MapGrid.PixelsPerCell),
                                                   new Vector2(0, 0), MapGrid.PixelsPerCell);
        ValidateMouse = validation;
    }

    private void MoveMouseSprite(Vector3 mousePosition)
    {
        if (MouseSpriteRenderer.gameObject.activeInHierarchy)
        {
            var cell = MapGrid.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));
            float x = cell.Coordinates.X;
            float y = cell.Coordinates.Y;

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