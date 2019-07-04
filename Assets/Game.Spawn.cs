using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Game // .Spawn
{
    public static void SpawnRune(CellData location, string name, Faction faction)
    {
        location.CellType = CellType.Stone;
        MapGrid.BindCell(location, "X");

        if (location.Structure != null)
            StructureController.DestroyStructure(location.Structure);

        var rune = StructureController.GetStructure(name, faction);
        location.CellType = CellType.Stone;
        MapGrid.BindCell(location, rune.Data.GetGameId());
        location.AddContent(rune.gameObject);
    }

    public static void InitialSpawn()
    {
        var midCell = MapGrid
            .GetCircle(new Coordinates(MapConstants.MapSize / 2, MapConstants.MapSize / 2), 10)
            .First(c => c.CellType != CellType.Water || c.CellType != CellType.Mountain);

        FactionController.PlayerFaction.transform.position = midCell.Coordinates.ToMapVector();

        if (midCell.Structure != null)
        {
            StructureController.DestroyStructure(midCell.Structure);
        }

        SummonCells(midCell, FactionController.PlayerFaction);
        CreateLeyLines();

        midCell.CellType = CellType.Mountain;

        CreatureController.SpawnPlayerAtLocation(midCell.GetNeighbor(Direction.E));
        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        for (int i = 0; i < MapConstants.MapSize / 10; i++)
        {
            var c = CreatureController.Beastiary.First().Value.CloneJson();
            c.Coordinates = MapGrid.GetRandomCell().Coordinates;
            CreatureController.SpawnCreature(c);

            FactionController.Factions[FactionConstants.Monster].AddCreature(c);
        }
    }

    private static void CreateLeyLines()
    {
        for (int i = 0; i < MapConstants.MapSize / 2; i++)
        {
            SpawnRune(MapGrid.GetRandomCell(), "BindRune", FactionController.WorldFaction);
        }

        var nexusPoints = new List<CellData>();
        for (int i = 0; i < MapConstants.CellsPerTerrainBlock * 2; i++)
        {
            var point = MapGrid.GetRandomCell();
            nexusPoints.Add(point);
            SpawnRune(point, "Pylon", FactionController.WorldFaction);
        }

        foreach (var cell in nexusPoints)
        {
            var target = nexusPoints[(int)(Random.value * (nexusPoints.Count - 1))];
            foreach (var path in Pathfinder.FindPath(cell, target, true))
            {
                if (path == target || path == cell)
                {
                    continue;
                }

                if (path.Structure != null)
                {
                    StructureController.DestroyStructure(path.Structure);
                }
                path.AddContent(StructureController.GetStructure("LeyLine", FactionController.WorldFaction).gameObject);
            }
        }
    }

    private static void SummonCells(CellData center, Faction faction)
    {
        center.CellType = CellType.Stone;
        MapGrid.BindCell(center, "X");

        foreach (var cell in center.Neighbors)
        {
            cell.CellType = CellType.Stone;
            MapGrid.BindCell(cell, "X");
        }

        SpawnRune(center.GetNeighbor(Direction.N), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.E), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.S), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.W), "BindRune", faction);
    }

}