public partial class OrderSelectionController //.Structure
{
    internal const string DefaultBuildText = "Select Building";

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
        }
    }
}