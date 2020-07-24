using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void ShowRoomInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
    }

    public void ShowAreaInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Area", "Select a location to place the Area.  Can be used to limit access to locations or designate areas for certain uses.");
    }

    public void ShowZoneInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
    }

    public void ShowStorageInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Storage", "Select a location to place the store.  Can be used to designate an area for storage of certain items.");
    }

    public void AddZoneClicked(Purpose purpose)
    {
        var sprite = Game.Instance.ZoneController.ZoneSprite;

        switch (purpose)
        {
            case Purpose.Room:
                sprite = "Room";
                ShowRoomInfo();
                break;

            case Purpose.Area:
                ShowAreaInfo();
                break;

            case Purpose.Storage:
                sprite = "Storage";
                ShowStorageInfo();
                break;
        }

        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(sprite), CanAddCellToZone);
        Game.Instance.Cursor.SetSelectionPreference(SelectionPreference.Cell);

        CellClickOrder = cells =>
        {
            var newZone = Game.Instance.ZoneController.Create(purpose, FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Instance.ShowZonePanel(newZone);
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

            CreateOrderButton(() => AddZoneClicked(Purpose.Room), () => ShowRoomInfo(), "beer_t");
            CreateOrderButton(() => AddZoneClicked(Purpose.Storage), () => ShowStorageInfo(), "box");
            CreateOrderButton(() => AddZoneClicked(Purpose.Area), () => ShowAreaInfo(), "plate_t");
        }
    }
}