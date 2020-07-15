using Assets.Structures;
using UnityEngine;

public partial class OrderSelectionController //.Construct
{
    internal OrderButton ConstructButton;

    private static void ShowConstructInfo(Construct constuct)
    {
        Game.Instance.OrderInfoPanel.Show(
             $"Place {constuct.Name}",
             "Select a location to place the construct, rotate with E or Q.  A construct is a predefined collection of structures and is built by a creature with the build skill.",
             constuct.Description,
             $"{constuct.TotalCost}"
            );
    }

    private void ConstructClicked(Construct constuct)
    {
        Debug.Log($"Construct clicked {constuct.Name}");

        Game.Instance.Cursor.ShowConstructGhost(constuct);
        ShowConstructInfo(constuct);

        CellClickOrder = cells =>
        {
            if (constuct.ValidateStartPos(cells[0]))
            {
                constuct.Place(cells[0], Game.Instance.FactionController.PlayerFaction);
            }
        };
    }

    public void ConstructTypeClicked()
    {
        Game.Instance.Cursor.SetSelectionPreference(SelectionPreference.Cell);
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var construct in Game.Instance.FileController.Constructs)
            {
                var title = $"{construct.Name} ({construct.Height}x{construct.Width})";
                var button = CreateOrderButton(() => ConstructClicked(construct),
                                               () => ShowConstructInfo(construct),
                                               construct.GetSprite());
                button.name = title;
            }
        }
    }
}