using Assets;
using Assets.ServiceLocator;
using System.Linq;

public partial class OrderSelectionController //.Designate
{
    internal const string AttackIcon = "war_t";
    internal const string DefaultRemoveIcon = "cancel";
    internal const string MoveIcon = "location_t";
    internal const string FollowIcon = "magnifier_t";

    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (Loc.GetGameController().OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(MoveClicked,
                              () => Loc.GetGameController().OrderInfoPanel.Show("Move to Cell", "Place a move order, a creature will take the order and move to the cell."),
                              MoveIcon);
            CreateOrderButton(RemoveStructureClicked,
                              () => Loc.GetGameController().OrderInfoPanel.Show("Remove structures", "Designate structures to be removed."),
                              DefaultRemoveIcon);
        }
    }

    private void MoveClicked()
    {
        Loc.Current.Get<CursorController>().SetSelectionPreference(SelectionPreference.Cell);
        Loc.Current.Get<CursorController>().SetSprite(Loc.GetSpriteStore().GetSprite(MoveIcon), (cell) => cell.TravelCost > 0);

        CellClickOrder = cells =>
        {
            var cell = cells[0];
            Loc.GetFactionController().PlayerFaction.AddTask(new Move(cell));
        };
    }

    private void RemoveStructureClicked()
    {
        Loc.Current.Get<CursorController>().SetSelectionPreference(SelectionPreference.Cell);
        Loc.Current.Get<CursorController>().SetSprite(Loc.GetSpriteStore().GetSprite(DefaultRemoveIcon), (cell) => cell.Structures != null);

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structures.Count > 0)
                {
                    var structure = cell.Structures.First();

                    if (Loc.GetFactionController().PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                    {
                        continue;
                    }
                    Loc.GetFactionController().PlayerFaction.AddTask(new RemoveStructure(structure));
                }
            }
        };
    }
}