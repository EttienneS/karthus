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

    public ZoneBase Create(Purpose purpose, string faction, params Cell[] cells)
    {
        ZoneBase newZone;
        var name = "New Zone";
        switch (purpose)
        {
            case Purpose.Room:
                newZone = new RoomZone();
                RoomZones.Add((RoomZone)newZone);
                name = $"Room {RoomZones.Count}";
                break;

            case Purpose.Area:
                newZone = new AreaZone();
                AreaZones.Add((AreaZone)newZone);
                name = $"Area {AreaZones.Count}";
                break;

            case Purpose.Storage:
                newZone = new StorageZone() { Filter = "*" };

                StorageZones.Add((StorageZone)newZone);
                name = $"Store {StorageZones.Count}";
                break;

            default:
                throw new NotImplementedException();
        }

        newZone.Cells = cells.ToList();
        newZone.Name = name;
        newZone.FactionName = faction;
        newZone.Purpose = purpose;

        Zones.Add(newZone, DrawZone(newZone));

        return newZone;
    }

    public void Delete(ZoneBase zone)
    {
        foreach (var cell in zone.Cells)
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

    public void Update()
    {
    }

    internal ZoneBase GetZoneForCell(Cell cell)
    {
        return Zones.Keys.FirstOrDefault(z => z.Cells.Contains(cell));
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

        foreach (var cell in newZone.Cells)
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

        var (bottomLeft, bottomRight, topLeft, topRight) = Game.Instance.Map.GetCorners(newZone.Cells);
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

    internal void Load(ZoneBase zone)
    {
        switch (zone.Purpose)
        {
            case Purpose.Area:
                var area = zone as AreaZone;
                AreaZones.Add(area);
                Zones.Add(area, DrawZone(area));
                break;
            case Purpose.Room:
                var room = zone as RoomZone;
                RoomZones.Add(room);
                Zones.Add(room, DrawZone(room));
                break;
            case Purpose.Storage:
                var storage = zone as StorageZone;
                StorageZones.Add(storage);
                Zones.Add(storage, DrawZone(storage));
                break;
        }

    }
}