public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddZoneClicked()
    {
       
    }

    public void ZoneTypeClicked()
    {
        if (Game.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            //foreach (var structureData in Game.StructureController.StructureDataReference.Values)
            //{
            //    if (!structureData.Buildable) continue;

            //    var button = CreateOrderButton(structureData.Name, () => BuildClicked(structureData.Name), structureData.SpriteName);
            //}
        }
    }
}