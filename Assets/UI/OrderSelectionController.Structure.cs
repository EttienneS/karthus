public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";

    internal OrderButton BuildButton;

    public void BuildClicked(string structureName)
    {
        var structure = Game.StructureController.StructureDataReference[structureName];
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.SetMouseSprite(structure.SpriteName,
                                      (cell) => structure.ValidateCellLocationForStructure(cell));

        Game.OrderInfoPanel.Title = $"Build {structureName}";
        Game.OrderInfoPanel.Description = "Select a location to place the structure.  A creature with the build skill will gather the required cost of material and then make the structure.";
        Game.OrderInfoPanel.Detail = structure.Description;
        Game.OrderInfoPanel.Cost = $"{structure.Cost}";
        Game.OrderInfoPanel.Show();

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (structure.ValidateCellLocationForStructure(cell))
                {
                    var blueprint = Game.StructureController.GetStructureBluePrint(structureName, cell, Game.FactionController.PlayerFaction);
                    Game.FactionController.PlayerFaction.AddTask(new Build(blueprint));
                }
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in Game.StructureController.StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = CreateOrderButton(structureData.Name, () => BuildClicked(structureData.Name), structureData.SpriteName);
            }
        }
    }
}