using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneController : MonoBehaviour
{
    public ZoneLabel ZoneLabelPrefab;
    public Tilemap ZoneTilemap;
    internal string RoomSprite = "Room";
    internal Sprite Sprite;
    internal string StorageSprite = "Storage";
    internal string ZoneSprite = "Zone";

    internal List<AreaZone> AreaZones { get; set; } = new List<AreaZone>();
    internal List<RoomZone> RoomZones { get; set; } = new List<RoomZone>();
    internal List<StorageZone> StorageZones { get; set; } = new List<StorageZone>();
    internal Dictionary<ZoneBase, ZoneLabel> Zones { get; set; } = new Dictionary<ZoneBase, ZoneLabel>();

    public void Awake()
    {
        Sprite = Game.Instance.SpriteStore.GetSprite(ZoneSprite);
    }
    public AreaZone CreateArea(string faction, params Cell[] cells)
    {
        var newZone =  new AreaZone();
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
        newZone.AddCells(cells.ToList());
        newZone.Name = name;
        newZone.FactionName = faction;

        Zones.Add(newZone, DrawZone(newZone));

    }

    public void Delete(ZoneBase zone)
    {
        foreach (var cell in zone.GetCells())
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
        return Zones.Keys.FirstOrDefault(z => z.GetCells().Contains(cell));
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

        foreach (var cell in newZone.GetCells())
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

        var (bottomLeft, bottomRight, topLeft, topRight) = Game.Instance.Map.GetCorners(newZone.GetCells());
        label.transform.localPosition = new Vector3((bottomLeft.X + bottomRight.X + topLeft.X + topRight.X) / 4f,
                                                    (bottomLeft.Z + bottomRight.Z + topLeft.Z + topRight.Z) / 4f, 0);
        label.transform.localPosition += new Vector3(0.5f, 0.5f);
        return label;
    }

    private void SetZoneCellTile(ZoneBase newZone, string sprite, Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.Instance.SpriteStore.GetSprite(sprite);
        tile.color = newZone.Color.ToColor();

        ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Z, 0), tile);
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
     
}