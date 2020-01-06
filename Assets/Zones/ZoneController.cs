using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneController : MonoBehaviour
{
    public Tilemap ZoneTilemap;
    public ZoneLabel ZoneLabelPrefab;

    internal string ZoneSprite = "Zone";
    internal Sprite Sprite;
    internal Dictionary<Zone, ZoneLabel> Zones { get; set; } = new Dictionary<Zone, ZoneLabel>();

    public Zone Create(params Cell[] cells)
    {
        var newZone = new Zone
        {
            Cells = cells.ToList(),
            Name = "New Zone"
        };

        Zones.Add(newZone, DrawZone(newZone));

        return newZone;
    }

    private ZoneLabel DrawZone(Zone newZone)
    {
        foreach (var cell in newZone.Cells)
        {
            SetZoneCellTile(newZone, cell);
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

    private void SetZoneCellTile(Zone newZone, Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.SpriteStore.GetSprite(ZoneSprite);
        tile.color = newZone.Color;

        ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
    }

    public void Awake()
    {
        Sprite = Game.SpriteStore.GetSprite(ZoneSprite);
    }

   

    public void Delete(Zone zone)
    {
        foreach (var cell in zone.Cells)
        {
            ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), null);
        }
        Destroy(Zones[zone].gameObject);
        Zones.Remove(zone);
    }

    public void Refresh(Zone zone)
    {
        Destroy(Zones[zone].gameObject);
        Zones[zone] = DrawZone(zone);
    }

    public void Update()
    {
    }

    internal Zone GetZoneForCell(Cell cell)
    {
        return Zones.Keys.FirstOrDefault(z => z.Cells.Contains(cell));
    }
}