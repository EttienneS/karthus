using Assets;
using Assets.ServiceLocator;
using Assets.Structures;
using UnityEngine;

public partial class OrderSelectionController //.Construct
{
    internal OrderButton ConstructButton;

    private static void ShowConstructInfo(Construct constuct)
    {
        Loc.GetGameController().OrderInfoPanel.Show(
             $"Place {constuct.Name}",
             "Select a location to place the construct, rotate with E or Q.  A construct is a predefined collection of structures and is built by a creature with the build skill.",
             constuct.Description,
             $"{constuct.TotalCost}"
            );
    }

    private void ConstructClicked(Construct constuct)
    {
        Debug.Log($"Construct clicked {constuct.Name}");

        Loc.Current.Get<CursorController>().ShowConstructGhost(constuct);
        ShowConstructInfo(constuct);

        CellClickOrder = cells =>
        {
            if (constuct.ValidateStartPos(cells[0]))
            {
                constuct.Place(cells[0], Loc.GetFactionController().PlayerFaction);
            }
        };
    }

    public void ConstructTypeClicked()
    {
        Loc.Current.Get<CursorController>().SetSelectionPreference(SelectionPreference.Cell);
        if (Loc.GetGameController().OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var construct in Loc.GetFileController().Constructs)
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