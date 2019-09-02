using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Core
{
    public List<CellData> Cells;
    public CellData Center;
    public int Radius;
    public List<Core> SubCores;

    public Core(CellData center, int radius)
    {
        Center = center;
        Radius = radius;

        Cells = Game.MapGrid.GetCircle(Center.Coordinates, radius);
        SubCores = new List<Core>();
    }

    public bool CellAvailable(CellData cell)
    {
        if (Cells.Contains(cell))
        {
            return false;
        }

        foreach (var core in SubCores)
        {
            if (core.Cells.Contains(cell))
            {
                return false;
            }
        }

        return true;
    }

    public void Propagte(float splitThreshold, int minLevel, float damper, Town town)
    {
        if (damper < 0.2f)
        {
            return;
        }

        for (int i = 0; i < minLevel; i++)
        {
            var angle = Game.MapGrid.GetAngle(town.Center.Coordinates, Center.Coordinates);

            var offPoint = Game.MapGrid.GetPointAtDistanceOnAngle(Center.Coordinates,
                                                                  Random.Range(Town.MinDistance, Town.MaxDistance),
                                                                  angle + Random.Range(-20f, 20f));

            if (offPoint == null)
            {
                continue;
            }
            if (!TryGetSubCore(Game.MapGrid.GetCellAtCoordinate(offPoint), town, out var core))
            {
                continue;
            }

            SubCores.Add(core);

            if (Random.value < splitThreshold)
            {
                core.Propagte(splitThreshold * damper, Mathf.FloorToInt(minLevel * damper), damper / 2, town);
            }
        }
    }

    public bool TryGetSubCore(CellData center, Town town, out Core core)
    {
        if (center == null)
        {
            core = null;
            return false;
        }
        core = new Core(center, Random.Range(Town.MinStructureSize, Town.MaxStructureSize));

        bool valid = true;
        foreach (var cell in core.Cells)
        {
            if (!CellAvailable(cell))
            {
                valid = false;
                break;
            }

            if (!town.CellAvailable(cell))
            {
                valid = false;
                break;
            }
        }

        return valid;
    }

    internal void Draw()
    {
        var originX = Center.Coordinates.X - (Radius / 2);
        var originY = Center.Coordinates.Y - (Radius / 2);

        var square = Game.MapGrid.GetRectangle(originX, originY, Radius, Radius);
        var subSquare = Game.MapGrid.GetRectangle(originX + 1, originY + 1, Radius - 2, Radius - 2);

        square.RemoveAll(c => subSquare.Contains(c));

        foreach (var cell in square)
        {
            cell.SetStructure(Game.StructureController.GetStructure("Stone Wall", FactionController.WorldFaction));
        }

        foreach (var cell in subSquare)
        {
            cell.SetStructure(Game.StructureController.GetStructure("Wood Tile", FactionController.WorldFaction));
        }

        foreach (var subCore in SubCores)
        {
            subCore.Draw();
        }

        Game.MapGenerator.MakeRune(Center, "BindRune", FactionController.WorldFaction);
        LinkCores();

        foreach (var cell in square)
        {
            if (cell.Neighbors.Count(c => c?.Structure?.Name == "Road") >= 2)
            {
                cell.SetStructure(Game.StructureController.GetStructure("Wood Tile", FactionController.WorldFaction));
                break;
            }
        }
    }

    internal void LinkCores()
    {
        foreach (var subCore in SubCores)
        {
            Game.MapGenerator.MakeRoad(Center, subCore.Center, false, MapGenerator.RoadSize.Single);
            subCore.LinkCores();
        }
    }

    internal bool Valid(Town town)
    {
        foreach (var cell in Cells)
        {
            if (!town.CellAvailable(cell))
            {
                return false;
            }
        }

        return true;
    }
}