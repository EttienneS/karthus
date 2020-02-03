using UnityEngine;

public partial class OrderSelectionController //.Construct
{
    public const string DefaultConstructText = "Place Construct";
    internal OrderButton ConstructButton;

    private void ConstructClicked(Construct constuct)
    {
        Debug.Log($"Construct clicked {constuct.Name}");

        ConstructButton.Text = DefaultConstructText;

        Game.Instance.SetConstructSprite(constuct.Texture, constuct.Width, constuct.Height,
                                       cell => constuct.ValidateStartPos(cell));

        Game.OrderInfoPanel.Title = $"Place {constuct.Name}";
        Game.OrderInfoPanel.Description = "Select a location to place the construct, rotate with E or Q.  A construct is a predefined collection of structures and is built by a creature with the build skill.";
        Game.OrderInfoPanel.Detail = constuct.Description;
        Game.OrderInfoPanel.Cost = $"{constuct.TotalCost}";
        Game.OrderInfoPanel.Show();

        Game.Instance.RotateMouseRight += () =>
        {
            constuct.RotateRight();
            Game.Instance.SetConstructSprite(constuct.GetTexture(), constuct.Width, constuct.Height,
                                           cell => constuct.ValidateStartPos(cell));
        };

        Game.Instance.RotateMouseLeft += () =>
        {
            constuct.RotateLeft();
            Game.Instance.SetConstructSprite(constuct.GetTexture(), constuct.Width, constuct.Height,
                                           cell => constuct.ValidateStartPos(cell));
        };

        CellClickOrder = cells =>
        {
            if (constuct.ValidateStartPos(cells[0]))
            {
                constuct.Place(cells[0], Game.FactionController.PlayerFaction);
            }
        };
    }

    private void ConstructTypeClicked()
    {
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
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