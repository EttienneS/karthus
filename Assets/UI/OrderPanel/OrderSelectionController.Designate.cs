﻿using System.Linq;

public partial class OrderSelectionController //.Designate
{
    internal const string AttackIcon = "war_t";
    internal const string DefaultRemoveIcon = "cancel";
    internal const string MoveIcon = "location_t";
    internal const string FollowIcon = "magnifier_t";

    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(MoveClicked,
                              () => Game.Instance.OrderInfoPanel.Show("Move to Cell", "Place a move order, a creature will take the order and move to the cell."),
                              MoveIcon);
            CreateOrderButton(RemoveStructureClicked,
                              () => Game.Instance.OrderInfoPanel.Show("Remove structures", "Designate structures to be removed."),
                              DefaultRemoveIcon);
        }
    }

    private void MoveClicked()
    {
        Game.Instance.Cursor.SetSelectionPreference(SelectionPreference.Cell);
        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(MoveIcon), (cell) => cell.TravelCost > 0);

        CellClickOrder = cells =>
        {
            var cell = cells[0];
            Game.Instance.FactionController.PlayerFaction.AddTask(new Move(cell));
        };
    }

    private void RemoveStructureClicked()
    {
        Game.Instance.Cursor.SetSelectionPreference(SelectionPreference.Cell);
        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(DefaultRemoveIcon), (cell) => cell.Structure != null);

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structure != null)
                {
                    var structure = cell.Structure;

                    if (Game.Instance.FactionController.PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                    {
                        continue;
                    }
                    Game.Instance.FactionController.PlayerFaction.AddTask(new RemoveStructure(structure));
                }
            }
        };
    }
}