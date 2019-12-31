using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneController : MonoBehaviour
{
    public Tilemap ZoneTilemap;
    public ZoneLabel ZoneLabelPrefab;

    internal List<Zone> Zones { get; set; } = new List<Zone>();

    public Zone Create(params Cell[] cells)
    {
        throw new NotImplementedException();
    }

    public void Delete(Zone zone)
    {
        throw new NotImplementedException();
    }

    public void Refresh(Zone zone)
    {
        throw new NotImplementedException();
    }
}
