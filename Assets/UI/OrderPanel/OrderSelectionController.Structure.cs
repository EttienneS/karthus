using Assets;
using Assets.ServiceLocator;

public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";

    internal OrderButton BuildButton;

    public void BuildClicked(string structureName)
    {
        var structure = Loc.GetStructureController().StructureDataReference[structureName];
        Loc.Current.Get<CursorController>().SetSelectionPreference(SelectionPreference.Cell);

        Loc.Current.Get<CursorController>().SetMesh(structureName, (cell) => structure.ValidateCellLocationForStructure(cell));
        UpdateStuctureOrder(structureName);

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    Loc.GetStructureController().SpawnBlueprint(structureName, cell, Loc.GetFactionController().PlayerFaction);
                }
            }
        };
    }

    public void UpdateStuctureOrder(string structureName)
    {
        var structure = Loc.GetStructureController().StructureDataReference[structureName];
        Loc.GetGameController().OrderInfoPanel.Show($"Build {structureName}",
                                           "Select a location to place the structure.  A creature with the build skill will gather the required cost of material and then make the structure.",
                                           structure.Description,
                                           $"{structure.Cost}");
    }


    public void BuildTypeClicked()
    {
        if (Loc.GetGameController().OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in Loc.GetStructureController().StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = CreateOrderButton(() => BuildClicked(structureData.Name), () => UpdateStuctureOrder(structureData.Name), structureData.Icon, structureData.ColorHex);
            }
        }
    }
}