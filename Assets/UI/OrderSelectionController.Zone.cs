using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddZoneClicked()
    {
        var zone = new Zone();
        Game.Controller.SetMouseSprite(Game.ZoneController.ZoneSprite,
                                       (cell) => CanAddCellToZone(cell, zone));
        Game.Controller.SelectionPreference = SelectionPreference.Cell;

        Game.OrderInfoPanel.Title = "Define Zone";
        Game.OrderInfoPanel.Description = "Select a location to place the zone.";
        // Game.OrderInfoPanel.Detail = structure.Description;
        // Game.OrderInfoPanel.Cost = $"{structure.Cost}";
        Game.OrderInfoPanel.Show();

        CellClickOrder = cells =>
        {
            Game.ZoneController.Create(cells.Where(c => CanAddCellToZone(c, zone)).ToArray());
        };
    }

    public bool CanAddCellToZone(Cell cell, Zone zone)
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

            CreateOrderButton("Add Zone", AddZoneClicked, Game.ZoneController.ZoneSprite);
        }
    }
}