public partial class OrderSelectionController //.Designate
{
    internal const string DefaultDesignateText = "Designate";

    internal const string CutText = "Cut Tree";
    internal const string CutIcon = "axe";

    internal const string HarvestText = "Harvest Plant";
    internal const string HarvestIcon = "wheat";

    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            TaskButton.Text = DefaultDesignateText;
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(CutText, CutTreeClicked, CutIcon);
            CreateOrderButton(HarvestText, HarvestClicked, HarvestIcon);
        }
    }

    private void HarvestClicked()
    {
        BuildButton.Text = CutText;
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null && cell.Data.Structure.StructureType == "Bush")
                {
                    cell.Data.Structure.LinkedGameObject.StatusSprite.sprite = SpriteStore.Instance.GetSpriteByName(HarvestIcon);
                }
            }
            GameController.Instance.DeselectCell();
        };
    }

    private void CutTreeClicked()
    {
        BuildButton.Text = CutText;
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null && cell.Data.Structure.StructureType == "Tree")
                {
                    var structure = cell.Data.Structure;
                    structure.LinkedGameObject.StatusSprite.sprite = SpriteStore.Instance.GetSpriteByName(CutIcon);
                }
            }
            GameController.Instance.DeselectCell();
        };
    }
}