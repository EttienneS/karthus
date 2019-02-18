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

            CreateOrderButton(CutText, () => HarvestClicked("Tree"), CutIcon);
            CreateOrderButton(HarvestText, () => HarvestClicked("Bush"), HarvestIcon);
        }
    }

    private void HarvestClicked(string type)
    {
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null && cell.Data.Structure.StructureType == type)
                {
                    cell.Data.Structure.LinkedGameObject.StatusSprite.sprite = SpriteStore.Instance.GetSpriteByName(HarvestIcon);
                    Taskmaster.Instance.AddTask(new Harvest(cell.Data.Structure));
                }
            }
            GameController.Instance.DeselectCell();
        };
    }
}