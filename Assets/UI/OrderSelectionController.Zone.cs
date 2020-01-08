using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddZoneClicked(Purpose purpose)
    {
        var sprite = Game.ZoneController.ZoneSprite;

        switch (purpose)
        {
            case Purpose.Room:
                Game.OrderInfoPanel.Title = "Define Room";
                sprite = "Room";
                Game.OrderInfoPanel.Description = "Select a location to place the Room, must be enclosed by walls.";
                break;

            case Purpose.Restriction:
                Game.OrderInfoPanel.Title = "Define Zone";
                Game.OrderInfoPanel.Description = "Select a location to place the zone.  Can be used to limit access to locations.";
                break;

            case Purpose.Storage:
                Game.OrderInfoPanel.Title = "Define Storage Area";
                sprite = "Storage";
                Game.OrderInfoPanel.Description = "Select a location to place the zone.  Can be used to designate an area for storage of certain items.";
                break;
        }

        Game.Controller.SetMouseSprite(sprite, (cell) => CanAddCellToZone(cell));

        Game.Controller.SelectionPreference = SelectionPreference.Cell;

        Game.OrderInfoPanel.Show();

        CellClickOrder = cells =>
        {
            var newZone = Game.ZoneController.Create(purpose, FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Controller.SelectZone(newZone);
        };
    }

    public bool CanAddCellToZone(Cell cell)
    {
        return true;
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

            CreateOrderButton("Add Room", () => AddZoneClicked(Purpose.Room), "beer_t");
            CreateOrderButton("Add Storage", () => AddZoneClicked(Purpose.Storage), "box");
            CreateOrderButton("Add Zone", () => AddZoneClicked(Purpose.Restriction), "plate_t");
        }
    }
}