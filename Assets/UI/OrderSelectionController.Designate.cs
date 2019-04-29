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
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            TaskButton.Text = DefaultDesignateText;
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(CutText, () => HarvestClicked("Tree", CutIcon), CutIcon);
            CreateOrderButton(HarvestText, () => HarvestClicked("Bush", HarvestIcon), HarvestIcon);
        }
    }

    private void HarvestClicked(string type, string icon)
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Bound && cell.Structure != null && cell.Structure.StructureType == type)
                {
                    cell.Structure.LinkedGameObject.StatusSprite.sprite = Game.SpriteStore.GetSpriteByName(icon);
                    Game.Taskmaster.AddTask(new Harvest(cell.Structure), string.Empty);
                }
            }
            Game.Controller.DeselectCell();
        };
    }
}