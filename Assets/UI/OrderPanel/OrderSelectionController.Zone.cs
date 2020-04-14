using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddZoneClicked(Purpose purpose)
    {
        var sprite = Game.Instance.ZoneController.ZoneSprite;

        switch (purpose)
        {
            case Purpose.Room:
                Game.Instance.OrderInfoPanel.Title = "Define Room";
                sprite = "Room";
                Game.Instance.OrderInfoPanel.Description = "Select a location to place the Room, must be enclosed by walls.";
                break;

            case Purpose.Area:
                Game.Instance.OrderInfoPanel.Title = "Define Area";
                Game.Instance.OrderInfoPanel.Description = "Select a location to place the Area.  Can be used to limit access to locations or designate areas for certain uses.";
                break;

            case Purpose.Storage:
                Game.Instance.OrderInfoPanel.Title = "Define Storage";
                sprite = "Storage";
                Game.Instance.OrderInfoPanel.Description = "Select a location to place the store.  Can be used to designate an area for storage of certain items.";
                break;
        }

        Game.Instance.SetMouseSprite(sprite, (cell) => CanAddCellToZone(cell));

        Game.Instance.SelectionPreference = SelectionPreference.Cell;

        Game.Instance.OrderInfoPanel.Show();

        CellClickOrder = cells =>
        {
            var newZone = Game.Instance.ZoneController.Create(purpose, FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Instance.SelectZone(newZone);
        };
    }

    public bool CanAddCellToZone(Cell cell)
    {
        return true;
    }

    public void ZoneTypeClicked()
    {
        if (Game.Instance.OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            CreateOrderButton("Add Room", () => AddZoneClicked(Purpose.Room), "beer_t");
            CreateOrderButton("Add Storage", () => AddZoneClicked(Purpose.Storage), "box");
            CreateOrderButton("Add Zone", () => AddZoneClicked(Purpose.Area), "plate_t");
        }
    }
}