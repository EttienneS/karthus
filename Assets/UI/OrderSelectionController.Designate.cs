using System.Linq;

public partial class OrderSelectionController //.Designate
{
    internal const string AttackIcon = "war_t";
    internal const string AttackText = "Attack";

    internal const string DefaultDesignateText = "Designate";

    internal const string DefaultRemoveIcon = "cancel";
    internal const string DefaultRemoveText = "Remove Building";

    internal const string MoveIcon = "location_t";
    internal const string MoveText = "Move";

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

            CreateOrderButton(MoveText, MoveClicked, MoveIcon);
            CreateOrderButton(DefaultRemoveText, RemoveStructureClicked, DefaultRemoveIcon);
        }
    }
    
    
    private void MoveClicked()
    {
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(MoveIcon, (c) => c.TravelCost > 0);

        CellClickOrder = cells =>
        {
            var cell = cells.First();
            Game.FactionController.PlayerFaction.AddTask(new Move(cell))
                                           .AddCellBadge(cell, MoveIcon);
        };
    }

    private void RemoveStructureClicked()
    {
        BuildButton.Text = DefaultRemoveText;
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structure?.Buildable == true)
                {
                    var structure = cell.Structure;

                    if (structure.IsBluePrint)
                    {
                        Game.StructureController.DestroyStructure(structure);
                    }
                    else
                    {
                        if (Game.FactionController.PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                        {
                            continue;
                        }
                        Game.FactionController.PlayerFaction
                                         .AddTask(new RemoveStructure(structure))
                                         .AddCellBadge(structure.Cell, DefaultRemoveIcon);
                    }
                }
            }
        };
    }
}