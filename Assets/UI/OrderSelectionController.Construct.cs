using UnityEngine;

public partial class OrderSelectionController //.Construct
{
    public const string DefaultConstructText = "Place Construct";
    internal OrderButton ConstructButton;

    private void ConstructClicked(Construct constuct)
    {
        Debug.Log($"Construct clicked {constuct.Name}");

        ConstructButton.Text = DefaultConstructText;

        Game.Controller.SetMouseSprite(constuct.Sprite, 1, 1, false,
                                       CellData => constuct.ValidateStartPos(CellData), 0.5f);

        //CellClickOrder = cells =>
        //{
        //    foreach (var cell in cells)
        //    {
        //        if (cell.Construct == null && cell.TravelCost > 0)
        //        {
        //            var Construct = ConstructController.AddConstruct(itemCategory);
        //            cell.AddContent(Construct.gameObject);
        //        }
        //    }
        //};
    }

    private void ConstructTypeClicked()
    {
        Debug.Log("Construct clicked");

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
                var button = CreateOrderButton(construct.Name, () => ConstructClicked(construct), construct.Sprite);
                button.name = construct.Name;
                button.Text = button.name;
            }
        }
    }
}