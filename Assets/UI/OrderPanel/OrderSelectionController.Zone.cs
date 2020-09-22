using Assets;
using Assets.ServiceLocator;
using System.Linq;

public partial class OrderSelectionController //.Zone
{
    internal const string DefaultZoneText = "Zones";

    internal OrderButton ZonesButton;

    public void AddAreaClicked()
    {
        ShowAreaInfo();
        SetCursorAndSpriteForZone(Loc.GetZoneController().ZoneSprite);

        CellClickOrder = cells =>
        {
            var newZone = Loc.GetZoneController().CreateArea(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Loc.GetGameController().ShowZonePanel(newZone);
        };
    }

    public void AddRoomClicked()
    {
        ShowRoomInfo();
        SetCursorAndSpriteForZone(Loc.GetZoneController().RoomSprite);

        CellClickOrder = cells =>
        {
            var newZone = Loc.GetZoneController().CreateRoom(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Loc.GetGameController().ShowZonePanel(newZone);
        };
    }

    public void AddStoreClicked()
    {
        ShowStorageInfo();
        SetCursorAndSpriteForZone(Loc.GetZoneController().StorageSprite);

        CellClickOrder = cells =>
        {
            var newZone = Loc.GetZoneController().CreateStore(FactionConstants.Player, cells.Where(c => CanAddCellToZone(c)).ToArray());
            Loc.GetGameController().ShowZonePanel(newZone);
        };
    }

    public void DeleteZoneClicked()
    {
        ShowDeleteInfo();
        SetCursorAndSpriteForZone(Loc.GetZoneController().RemoveSprite);

        CellClickOrder = cells => Loc.GetZoneController().ClearZonesFromCells(cells);
    }

    public bool CanAddCellToZone(Cell cell)
    {
        return true;
    }

    public void ShowDeleteInfo()
    {
        Loc.GetGameController().OrderInfoPanel.Show("Define Area", "Select cells to delete Zones from.");
    }

    public void ShowAreaInfo()
    {
        Loc.GetGameController().OrderInfoPanel.Show("Define Area", "Select a location to place the Area.  Can be used to limit access to locations or designate areas for certain uses.");
    }

    public void ShowRoomInfo()
    {
        Loc.GetGameController().OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
    }

    public void ShowStorageInfo()
    {
        Loc.GetGameController().OrderInfoPanel.Show("Define Storage", "Select a location to place the store.  Can be used to designate an area for storage of certain items.");
    }

    public void ShowZoneInfo()
    {
        Loc.GetGameController().OrderInfoPanel.Show("Define Room", "Select a location to place the Room, must be enclosed by walls.");
    }

    public void ZoneTypeClicked()
    {
        if (Loc.GetGameController().OrderTrayController.gameObject.activeInHierarchy)
        {
            DisableAndReset();
        }
        else
        {
            EnableAndClear();

            CreateOrderButton(() => AddRoomClicked(), () => ShowRoomInfo(), "beer_t");
            CreateOrderButton(() => AddStoreClicked(), () => ShowStorageInfo(), "box");
            CreateOrderButton(() => AddAreaClicked(), () => ShowAreaInfo(), "plate_t");
            CreateOrderButton(() => DeleteZoneClicked(), () => ShowDeleteInfo(), "cancel");
        }
    }

    private void SetCursorAndSpriteForZone(string sprite)
    {
        Loc.Current.Get<CursorController>().SetSprite(Loc.GetSpriteStore().GetSprite(sprite), CanAddCellToZone);
        Loc.Current.Get<CursorController>().SetSelectionPreference(SelectionPreference.Cell);
    }
}