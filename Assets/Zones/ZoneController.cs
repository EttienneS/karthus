using Assets.Map;
using Assets.ServiceLocator;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneController : MonoBehaviour, IGameService
{
    public ZoneLabel ZoneLabelPrefab;
    public Tilemap ZoneTilemap;

    internal Sprite Sprite;

    internal string RoomSprite = "Room";
    internal string StorageSprite = "Storage";
    internal string ZoneSprite = "Zone";
    internal string RemoveSprite = "RemoveZone";

    internal List<AreaZone> AreaZones { get; set; } = new List<AreaZone>();
    internal List<RoomZone> RoomZones { get; set; } = new List<RoomZone>();
    internal List<StorageZone> StorageZones { get; set; } = new List<StorageZone>();
    internal Dictionary<ZoneBase, ZoneLabel> Zones { get; set; } = new Dictionary<ZoneBase, ZoneLabel>();

    public AreaZone CreateArea(string faction, params Cell[] cells)
    {
        var newZone = new AreaZone();
        AreaZones.Add(newZone);
        name = $"Area {AreaZones.Count}";
        AssignAndPopulateZone(faction, cells, newZone);
        return newZone;
    }

    public StorageZone CreateStore(string faction, params Cell[] cells)
    {
        var newZone = new StorageZone();
        StorageZones.Add(newZone);
        name = $"Store {StorageZones.Count}";
        AssignAndPopulateZone(faction, cells, newZone);
        return newZone;
    }

    public RoomZone CreateRoom(string faction, params Cell[] cells)
    {
        var newZone = new RoomZone();
        RoomZones.Add(newZone);
        name = $"Room {RoomZones.Count}";
        AssignAndPopulateZone(faction, cells, newZone);

        return newZone;
    }

    private void AssignAndPopulateZone(string faction, Cell[] cells, ZoneBase newZone)
    {
        ClearZonesFromCells(cells);
        newZone.ZoneCells.AddCells(cells.ToList());
        newZone.Name = name;
        newZone.FactionName = faction;

        Zones.Add(newZone, DrawZone(newZone));
    }

    public void ClearZonesFromCells(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            var current = Game.Instance.ZoneController.GetZoneForCell(cell);
            if (current != null)
            {
                current.ZoneCells.RemoveCell(cell);
                ClearZoneCellTile(cell);

                if (current.ZoneCells.GetCells().Count == 0)
                {
                    Delete(current);
                }
                else
                {
                    MoveZoneLabel(current);
                }
            }
        }
    }

    public void Delete(ZoneBase zone)
    {
        foreach (var cell in zone.ZoneCells.GetCells())
        {
            ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Z, 0), null);
        }
        Destroy(Zones[zone].gameObject);
        Zones.Remove(zone);

        if (zone is RoomZone rz)
        {
            RoomZones.Remove(rz);
        }
        else if (zone is StorageZone sz)
        {
            StorageZones.Remove(sz);
        }
        else if (zone is AreaZone rez)
        {
            AreaZones.Remove(rez);
        }
    }

    public void Refresh(ZoneBase zone)
    {
        Destroy(Zones[zone].gameObject);
        Zones[zone] = DrawZone(zone);
    }

    internal ZoneBase GetZoneForCell(Cell cell)
    {
        return Zones.Keys.FirstOrDefault(z => z.ZoneCells.GetCells().Contains(cell));
    }

    private ZoneLabel DrawZone(ZoneBase newZone)
    {
        var sprite = Game.Instance.ZoneController.ZoneSprite;
        var room = false;
        if (newZone is RoomZone)
        {
            sprite = Game.Instance.ZoneController.RoomSprite;
            room = true;
        }
        else if (newZone is StorageZone)
        {
            sprite = Game.Instance.ZoneController.StorageSprite;
        }

        foreach (var cell in newZone.ZoneCells.GetCells())
        {
            SetZoneCellTile(newZone, sprite, cell);
            if (room)
            {
                Game.Instance.StructureController.CreateRoof(cell);
            }
        }

        var label = Instantiate(ZoneLabelPrefab, transform);
        label.name = newZone.Name;
        label.Text.text = newZone.Name;
        MoveZoneLabel(newZone, label);
        return label;
    }

    private void MoveZoneLabel(ZoneBase zone)
    {
        MoveZoneLabel(zone, Zones[zone]);
    }

    private void MoveZoneLabel(ZoneBase zone, ZoneLabel label)
    {
        var (bottomLeft, bottomRight, topLeft, topRight) = MapController.Instance.GetCorners(zone.ZoneCells.GetCells());
        label.transform.localPosition = new Vector3((bottomLeft.X + bottomRight.X + topLeft.X + topRight.X) / 4f,
                                                    (bottomLeft.Z + bottomRight.Z + topLeft.Z + topRight.Z) / 4f, 0);
        label.transform.localPosition += new Vector3(0.5f, 0.5f);
    }

    private void SetZoneCellTile(ZoneBase newZone, string sprite, Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.Instance.SpriteStore.GetSprite(sprite);
        tile.color = newZone.ColorString.GetColorFromHex();

        ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Z, 0), tile);
    }

    private void ClearZoneCellTile(Cell cell)
    {
        ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Z, 0), null);
    }

    internal void LoadArea(AreaZone area)
    {
        AreaZones.Add(area);
        Zones.Add(area, DrawZone(area));
    }

    internal void LoadRoom(RoomZone room)
    {
        RoomZones.Add(room);
        Zones.Add(room, DrawZone(room));
    }

    internal void LoadStore(StorageZone storage)
    {
        StorageZones.Add(storage);
        Zones.Add(storage, DrawZone(storage));
    }

    public void Initialize()
    {
        Sprite = Game.Instance.SpriteStore.GetSprite(ZoneSprite);
    }
}