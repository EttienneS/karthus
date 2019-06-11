using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class OrderSelectionController //.Structure
{
    internal OrderButton BuildButton;
    private const string DefaultBuildText = "Select Building";
    private const string DefaultRemoveText = "Remove Building";

    public void BuildClicked(string structureName)
    {
        BuildButton.Text = "Build " + structureName;

        var structure = Game.StructureController.StructureDataReference[structureName];
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(Game.SpriteStore.GetSpriteByName(structure.SpriteName).texture,
                                       structure.Width, structure.Height,
                                       (CellData) => structure.ValidateCellLocationForStructure(CellData));

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    var blueprint = Game.StructureController.GetStructureBluePrint(structureName);
                    cell.AddContent(blueprint.gameObject);
                    FactionController.Factions[FactionConstants.Player].AddTask(new Build(blueprint.Data, cell.Coordinates), string.Empty);
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
                if (structureData.Tiled)
                {
                    button.Button.image.type = Image.Type.Tiled;
                }
            }

            CreateOrderButton(DefaultRemoveText, RemoveStructureClicked, "cancel");
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
                        if (FactionController.Factions[FactionConstants.Player].Tasks.OfType<RemoveStructure>().Any(t => t.Structure == structure))
                        {
                            Debug.Log("Structure already flagged to remove");
                            continue;
                        }
                        FactionController.Factions[FactionConstants.Player].AddTask(new RemoveStructure(structure, cell.Coordinates), string.Empty);
                        structure.LinkedGameObject.SpriteRenderer.color = ColorConstants.InvalidColor;
                    }
                }
            }
        };
    }
}