public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";

    internal OrderButton BuildButton;

    public void BuildClicked(string structureName)
    {
        var structure = Game.Instance.StructureController.StructureDataReference[structureName];
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.SetMouseSprite(structure.SpriteName,
                                      (cell) => structure.ValidateCellLocationForStructure(cell));

        Game.Instance.OrderInfoPanel.Title = $"Build {structureName}";
        Game.Instance.OrderInfoPanel.Description = "Select a location to place the structure.  A creature with the build skill will gather the required cost of material and then make the structure.";
        Game.Instance.OrderInfoPanel.Detail = structure.Description;
        Game.Instance.OrderInfoPanel.Cost = $"{structure.Cost}";
        Game.Instance.OrderInfoPanel.Show();

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    var blueprint = Game.Instance.StructureController.GetStructureBluePrint(structureName, cell, Game.Instance.FactionController.PlayerFaction);
                    Game.Instance.FactionController.PlayerFaction.AddTask(new Build(blueprint));
                }
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in Game.Instance.StructureController.StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = CreateOrderButton(structureData.Name, () => BuildClicked(structureData.Name), structureData.SpriteName);
            }
        }
    }
}