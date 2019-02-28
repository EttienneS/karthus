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
        GameController.Instance.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structure == null)
                {
                    var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
                    cell.AddContent(blueprint.gameObject);
                    Taskmaster.Instance.AddTask(new Build(blueprint.Data, cell.Coordinates));
                }
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();

            BuildButton.Text = DefaultBuildText;
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in StructureController.Instance.StructureDataReference.Values)
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
        GameController.Instance.SelectionPreference = SelectionPreference.Cell;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Structure?.Buildable == true)
                {
                    var structure = cell.Structure;

                    if (structure.IsBluePrint)
                    {
                        StructureController.Instance.DestroyStructure(structure);
                    }
                    else
                    {
                        Taskmaster.Instance.AddTask(new RemoveStructure(structure, cell.Coordinates));
                        structure.LinkedGameObject.SpriteRenderer.color = Color.red;
                    }
                }
            }
        };
    }
}