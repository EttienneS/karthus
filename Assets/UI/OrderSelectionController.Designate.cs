public partial class OrderSelectionController //.Designate
{
    public const string DefaultDesignateText = "Designate";
    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            TaskButton.Text = "Designate";
        }
        else
        {
            EnableAndClear();

            var cutButton = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
            cutButton.Button.onClick.AddListener(CutTreeClicked);
            cutButton.name = "Cut Tree";
            cutButton.Text = cutButton.name;
            cutButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("axe");
        }
    }

    private void CutTreeClicked()
    {
        BuildButton.Text = "Cut Tree";
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null)
                {
                    var structure = cell.Data.Structure;
                    structure.LinkedGameObject.StatusSprite.sprite = SpriteStore.Instance.GetSpriteByName("axe");
                }
            }
            GameController.Instance.DeselectCell();
        };
    }
}