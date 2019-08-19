using UnityEngine;

public partial class OrderSelectionController //.Construct
{
    public const string DefaultConstructText = "Place Construct";
    internal OrderButton ConstructButton;

    private void ConstructClicked(Construct constuct)
    {
        Debug.Log($"Construct clicked {constuct.Name}");

        ConstructButton.Text = DefaultConstructText;

        Game.Controller.SetMouseSprite(constuct.Texture, constuct.Width, constuct.Height,
                                       CellData => constuct.ValidateStartPos(CellData));

        Game.Controller.RotateMouseRight += () =>
        {
            constuct.RotateRight();
            Game.Controller.SetMouseSprite(constuct.GetTexture(), constuct.Width, constuct.Height,
                                           CellData => constuct.ValidateStartPos(CellData));
        };

        Game.Controller.RotateMouseLeft += () =>
        {
            constuct.RotateLeft();
            Game.Controller.SetMouseSprite(constuct.GetTexture(), constuct.Width, constuct.Height,
                                           CellData => constuct.ValidateStartPos(CellData));
        };

        CellClickOrder = cells =>
        {
            if (constuct.ValidateStartPos(cells[0]))
            {
                constuct.Place(cells[0], FactionController.PlayerFaction);
            }
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
                var title = $"{construct.Name} ({construct.Height}x{construct.Width})";
                var button = CreateOrderButton(title, () => ConstructClicked(construct), construct.Sprite);
                button.name = title;
                button.Text = title;
            }
        }
    }
}