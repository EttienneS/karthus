using System;
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
    internal List<Zone> Zones { get; set; } = new List<Zone>();

    public Zone Create(params Cell[] cells)
    {
        var newZone = new Zone
        {
            Cells = cells.ToList(),
            Name = "New Zone"
        };
        Zones.Add(newZone);

        foreach (var cell in newZone.Cells)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Game.SpriteStore.GetSprite(ZoneSprite);
            tile.color = newZone.Color;

            ZoneTilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
        }

        var label = Instantiate(ZoneLabelPrefab, transform);
        label.name = newZone.Name;
        label.Text.text = newZone.Name;

        var (bottomLeft, bottomRight, topLeft, topRight) = Game.MapGenerator.GetCorners(newZone.Cells);
        label.transform.position = new Vector3((bottomLeft.X + bottomRight.X + topLeft.X + topRight.X) / 4f, 
                                               (bottomLeft.Y + bottomRight.Y + topLeft.Y + topRight.Y) / 4f, 0);
        label.transform.position += new Vector3(0.5f, 0.5f);
        return newZone;
    }

    public void Awake()
    {
        Sprite = Game.SpriteStore.GetSprite(ZoneSprite);
    }

    public void Delete(Zone zone)
    {
        throw new NotImplementedException();
    }

    public void Refresh(Zone zone)
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
    }
}