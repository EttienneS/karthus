public partial class OrderSelectionController //.Construct
{
    public const string DefaultConstructText = "Place Construct";
    internal OrderButton ConstructButton;

    private void ConstructClicked(Construct constuct)
    {
        ConstructButton.Text = DefaultConstructText;

        CellClickOrder = cells =>
        {
            //foreach (var cell in cells)
            //{
            //    if (cell.Construct == null && cell.TravelCost > 0)
            //    {
            //        var Construct = Game.ConstructController.AddConstruct(itemCategory);
            //        cell.AddContent(Construct.gameObject);
            //    }
            //}
        };
    }

    private void ConstructTypeClicked()
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            ConstructButton.Text = DefaultConstructText;
        }
        else
        {
            EnableAndClear();

            foreach (var construct in ConstructController.Constructs)
            {
                var button = CreateOrderButton(construct.Name, () => ConstructClicked(construct), construct.SpriteName);
                button.name = construct.Name;
                button.Text = button.name;
            }
        }
    }
}