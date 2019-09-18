using System.Linq;
using UnityEngine;

public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";
    internal const string DefaultRemoveImage = "cancel";
    internal const string DefaultRemoveText = "Remove Building";
    internal OrderButton BuildButton;

    public void BuildClicked(string structureName)
    {
        BuildButton.Text = "Build " + structureName;

        var structure = Game.StructureController.StructureDataReference[structureName];
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(structure.SpriteName,
                                       (CellData) => structure.ValidateCellLocationForStructure(CellData));

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    var blueprint = Game.StructureController.GetStructureBluePrint(structureName, FactionController.PlayerFaction);
                    cell.SetStructure(blueprint);
                    cell.UpdateTile();
                    FactionController.PlayerFaction.AddTask(new Build(blueprint), null);
                }
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();

            BuildButton.Text = DefaultBuildText;
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in Game.StructureController.StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = CreateOrderButton(structureData.Name, () => BuildClicked(structureData.Name), structureData.SpriteName);
            }

            CreateOrderButton(DefaultRemoveText, RemoveStructureClicked, DefaultRemoveImage);
        }
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
                        if (FactionController.PlayerFaction.Tasks.OfType<RemoveStructure>().Any(t => t.Structure == structure))
                        {
                            Debug.Log("Structure already flagged to remove");
                            continue;
                        }
                        FactionController.PlayerFaction
                                         .AddTaskWithCellBadge(
                                                new RemoveStructure(structure),
                                                null,
                                                structure.Cell,
                                                DefaultRemoveImage);
                    }
                }
            }
        };
    }
}