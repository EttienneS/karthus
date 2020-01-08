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
    internal List<RestrictionZone> RestrictionZones { get; set; } = new List<RestrictionZone>();
    internal List<RoomZone> RoomZones { get; set; } = new List<RoomZone>();
    internal List<StorageZone> StorageZones { get; set; } = new List<StorageZone>();
    internal Dictionary<ZoneBase, ZoneLabel> Zones { get; set; } = new Dictionary<ZoneBase, ZoneLabel>();

    public void Awake()
    {
        Sprite = Game.SpriteStore.GetSprite(ZoneSprite);
    }

    public ZoneBase Create(Purpose purpose, string faction, params Cell[] cells)
    {
        ZoneBase newZone;
        switch (purpose)
        {
            case Purpose.Room:
                newZone = new RoomZone();
                RoomZones.Add((RoomZone)newZone);
                break;

            case Purpose.Restriction:
                newZone = new RestrictionZone();
                RestrictionZones.Add((RestrictionZone)newZone);
                break;

            case Purpose.Storage:
                newZone = new StorageZone();
                StorageZones.Add((StorageZone)newZone);

                break;

            default:
                throw new NotImplementedException();
        }

        newZone.Cells = cells.ToList();
        newZone.Name = "New Zone";
        newZone.FactionName = faction;

        Zones.Add(newZone, DrawZone(newZone));

        return newZone;
    }

    public void Delete(ZoneBase zone)
    {
        foreach (var cell in zone.Cells)
        {
            ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), null);
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
        else if (zone is RestrictionZone rez)
        {
            RestrictionZones.Remove(rez);
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
        var sprite = Game.ZoneController.ZoneSprite;
        if (newZone is RoomZone)
        {
            sprite = Game.ZoneController.RoomSprite;
        }
        else if (newZone is StorageZone)
        {
            sprite = Game.ZoneController.StorageSprite;
        }

        foreach (var cell in newZone.Cells)
        {
            SetZoneCellTile(newZone, sprite, cell);
        }

        var label = Instantiate(ZoneLabelPrefab, transform);
        label.name = newZone.Name;
        label.Text.text = newZone.Name;

        var (bottomLeft, bottomRight, topLeft, topRight) = Game.MapGenerator.GetCorners(newZone.Cells);
        label.transform.position = new Vector3((bottomLeft.X + bottomRight.X + topLeft.X + topRight.X) / 4f,
                                               (bottomLeft.Y + bottomRight.Y + topLeft.Y + topRight.Y) / 4f, 0);
        label.transform.position += new Vector3(0.5f, 0.5f);
        return label;
    }

    private void SetZoneCellTile(ZoneBase newZone, string sprite, Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.SpriteStore.GetSprite(sprite);
        tile.color = newZone.Color;

        ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
    }
}