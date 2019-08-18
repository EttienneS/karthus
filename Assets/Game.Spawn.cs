using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Game // .Spawn
{
    public static void SpawnRune(CellData location, string name, Faction faction)
    {
        location.CellType = CellType.Stone;
        MapGrid.BindCell(location, faction.Core);

        if (location.Structure != null)
            StructureController.DestroyStructure(location.Structure);

        var rune = StructureController.GetStructure(name, faction);
        location.CellType = CellType.Stone;
        MapGrid.BindCell(location, rune);
        location.SetStructure(rune);
    }

    public static void InitialSpawn()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var midCell = MapGrid
            .GetCircle(new Coordinates(Game.MapGrid.MapSize / 2, Game.MapGrid.MapSize / 2), 10)
            .First(c => c.CellType != CellType.Water || c.CellType != CellType.Mountain);

        if (midCell.Structure != null)
        {
            StructureController.DestroyStructure(midCell.Structure);
        }

        SummonCells(midCell, FactionController.PlayerFaction);
        CreateLeyLines();

        midCell.SetStructure(FactionController.PlayerFaction.Core);

        CreatureController.SpawnCreature(CreatureController.GetCreatureOfType("Person"),
                                         midCell.GetNeighbor(Direction.E).Coordinates,
                                         FactionController.PlayerFaction);

        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        for (int i = 0; i < MapGrid.MapSize; i++)
        {
            CreatureController.SpawnCreature(CreatureController.GetCreatureOfType("AbyssWraith"),
                                             MapGrid.GetRandomCell().Coordinates,
                                             FactionController.MonsterFaction);
        }

        Debug.Log($"Did initial spawn in {sw.Elapsed}s");
        sw.Stop();
    }

    private static void CreateLeyLines()
    {
        for (int i = 0; i < Game.MapGrid.MapSize / 2; i++)
        {
            SpawnRune(MapGrid.GetRandomCell(), "BindRune", FactionController.WorldFaction);
        }

        var nexusPoints = new List<CellData>();
        for (int i = 0; i < Game.MapGrid.MapSize / 10; i++)
        {
            var point = MapGrid.GetRandomCell();
            nexusPoints.Add(point);
            SpawnRune(point, "Pylon", FactionController.WorldFaction);
        }

        foreach (var cell in nexusPoints)
        {
            var target = nexusPoints[(int)(Random.value * (nexusPoints.Count - 1))];
            LeyLineController.MakeLine(Pathfinder.FindPath(cell, target, Mobility.Fly), Helpers.RandomEnumValue<ManaColor>());
        }
    }

    private static void SummonCells(CellData center, Faction faction)
    {
        center.CellType = CellType.Stone;
        MapGrid.BindCell(center, faction.Core);

        foreach (var cell in center.Neighbors)
        {
            cell.CellType = CellType.Stone;
            MapGrid.BindCell(cell, faction.Core);
        }

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W), "BindRune", faction);
    }
}