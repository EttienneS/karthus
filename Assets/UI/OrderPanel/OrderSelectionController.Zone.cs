using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddAreaClicked()
    {
        ShowAreaInfo();
        SetCursorAndSpriteForZone(Game.Instance.ZoneController.ZoneSprite);

        CellClickOrder = cells =>
        {
            var newZone = Game.Instance.ZoneController.CreateArea(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Instance.ShowZonePanel(newZone);
        };
    }

    public void AddRoomClicked()
    {
        ShowRoomInfo();
        SetCursorAndSpriteForZone(Game.Instance.ZoneController.RoomSprite);

        CellClickOrder = cells =>
        {
            var newZone = Game.Instance.ZoneController.CreateRoom(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Instance.ShowZonePanel(newZone);
        };
    }

    public void AddStoreClicked()
    {
        ShowStorageInfo();
        SetCursorAndSpriteForZone(Game.Instance.ZoneController.StorageSprite);

        CellClickOrder = cells =>
        {
            var newZone = Game.Instance.ZoneController.CreateStore(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Game.Instance.ShowZonePanel(newZone);
        };
    }

    public bool CanAddCellToZone(Cell cell)
    {
        return true;
    }

    public void ShowAreaInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Area", "Select a location to place the Area.  Can be used to limit access to locations or designate areas for certain uses.");
    }

    public void ShowRoomInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
    }

    public void ShowStorageInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Storage", "Select a location to place the store.  Can be used to designate an area for storage of certain items.");
    }

    public void ShowZoneInfo()
    {
        Game.Instance.OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
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

            CreateOrderButton(() => AddRoomClicked(), () => ShowRoomInfo(), "beer_t");
            CreateOrderButton(() => AddStoreClicked(), () => ShowStorageInfo(), "box");
            CreateOrderButton(() => AddAreaClicked(), () => ShowAreaInfo(), "plate_t");
        }
    }

    private void SetCursorAndSpriteForZone(string sprite)
    {
        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(sprite), CanAddCellToZone);
        Game.Instance.Cursor.SetSelectionPreference(SelectionPreference.Cell);
    }
}