using System.Linq;

public partial class OrderSelectionController //.Designate
{
    internal const string AttackIcon = "war_t";
    internal const string AttackText = "Attack";
    internal const string CancelText = "Cancel";

    internal const string DefaultDesignateText = "Designate";

    internal const string DefaultRemoveIcon = "cancel";
    internal const string DefaultRemoveText = "Remove Building";

    internal const string MoveIcon = "location_t";
    internal const string MoveText = "Move";

    internal OrderButton TaskButton;

    public void DesignateTypeClicked()
    {
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
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
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.SetMouseSprite(MoveIcon, (cell) => cell.TravelCost > 0);

        CellClickOrder = cells =>
        {
            var cell = cells[0];
            Game.Instance.FactionController.PlayerFaction.AddTask(new Move(cell))
                                                .AddCellBadge(cell, MoveIcon);
        };
    }

    private void RemoveStructureClicked()
    {
        BuildButton.Text = DefaultRemoveText;
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.SetMouseSprite(DefaultRemoveIcon, (cell) => cell.Structure != null);

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structure != null)
                {
                    var structure = cell.Structure;

                    if (structure.IsBluePrint)
                    {
                        var task = structure.Faction.AvailableTasks.OfType<Build>().FirstOrDefault(b => b.TargetStructure == structure);
                        if (task != null)
                        {
                            structure.Faction.RemoveTask(task);
                            task.Destroy();
                        }
                        else
                        {
                            foreach (var creature in structure.Faction.Creatures)
                            {
                                if (creature.Task != null && task is Build build)
                                {
                                    if (build.TargetStructure == structure)
                                    {
                                        creature.CancelTask();
                                    }
                                }
                            }
                        }

                        Game.Instance.StructureController.DestroyStructure(structure);
                    }
                    else
                    {
                        if (Game.Instance.FactionController.PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                        {
                            continue;
                        }
                        Game.Instance.FactionController.PlayerFaction
                                              .AddTask(new RemoveStructure(structure))
                                              .AddCellBadge(structure.Cell, DefaultRemoveIcon);
                    }
                }
            }
        };
    }
}