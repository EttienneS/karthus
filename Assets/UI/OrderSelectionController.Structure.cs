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
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure == null)
                {
                    var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
                    cell.AddContent(blueprint.gameObject);
                    Taskmaster.Instance.AddTask(new Build(blueprint.Data, cell.Data.Coordinates));
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

                var button = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
                button.Button.onClick.AddListener(() => BuildClicked(structureData.Name));
                button.name = structureData.Name;
                button.Button.image.sprite = StructureController.Instance.GetSpriteForStructure(structureData.Name);

                if (structureData.Tiled)
                {
                    button.Button.image.type = Image.Type.Tiled;
                }

                button.Text = "Build " + structureData.Name;
            }

            var removeButton = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
            removeButton.Button.onClick.AddListener(RemoveStructureClicked);
            removeButton.name = DefaultRemoveText;
            removeButton.Text = removeButton.name;
            removeButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("cancel");
        }
    }

    private void RemoveStructureClicked()
    {
        BuildButton.Text = DefaultRemoveText;
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure?.Buildable == true)
                {
                    var structure = cell.Data.Structure;

                    if (structure.IsBluePrint)
                    {
                        StructureController.Instance.DestroyStructure(structure);
                    }
                    else
                    {
                        Taskmaster.Instance.AddTask(new RemoveStructure(structure, cell.Data.Coordinates));
                        structure.LinkedGameObject.SpriteRenderer.color = Color.red;
                    }
                }
            }
        };
    }
}