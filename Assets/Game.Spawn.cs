using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class Game // .Spawn
{
    public static Structure SpawnRune(CellData location, string name, Faction faction)
    {
        MapGrid.BindCell(location, faction.Core);

        if (location.Structure != null)
            StructureController.DestroyStructure(location.Structure);

        var rune = StructureController.GetStructure(name, faction);
        MapGrid.BindCell(location, rune);
        location.SetStructure(rune);

        if (name == "BindRune")
        {
            foreach (var c in MapGrid.BleedGroup(MapGrid.GetCircle(location.Coordinates, Random.Range(4, 7))))
            {
                MapGrid.BindCell(c, rune);
            }
        }

        if (rune.Spell != null)
        {
            Game.MagicController.AddRune(rune);
        }


        return rune;
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

        for (int i = 0; i < 3; i++)
        {
            CreatureController.SpawnCreature(CreatureController.GetCreatureOfType("Person"),
                                         midCell.GetNeighbor(Helpers.RandomEnumValue<Direction>()).Coordinates,
                                         FactionController.PlayerFaction);
        }

        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        for (int i = 0; i < MapGrid.MapSize; i++)
        {
            CreatureController.SpawnCreature(CreatureController.GetCreatureOfType("AbyssWraith"),
                                             MapGrid.GetRandomCell().Coordinates,
                                             FactionController.MonsterFaction);
        }

        sw.Stop();

        MapGrid.ProcessBindings(MapGrid.PendingBinding.Count * 200);

        Debug.Log($"Did initial spawn in {sw.Elapsed}s");
    }

    private static void CreateLeyLines()
    {
        for (int i = 0; i < MapGrid.MapSize / 2; i++)
        {
            var cell = MapGrid.GetRandomCell();
            SpawnRune(cell, "BindRune", FactionController.WorldFaction);
        }

        var nexusPoints = new List<CellData>();
        for (int i = 0; i < Game.MapGrid.MapSize / 10; i++)
        {
            var point = MapGrid.GetRandomCell();
            nexusPoints.Add(point);
            SpawnRune(point, "Pylon", FactionController.WorldFaction);
        }

        var v = Enum.GetValues(typeof(ManaColor));
        var counter = 0;
        foreach (var cell in nexusPoints)
        {
            var target = nexusPoints[(int)(Random.value * (nexusPoints.Count - 1))];
            LeyLineController.MakeLine(Pathfinder.FindPath(cell, target, Mobility.Fly), (ManaColor)v.GetValue(counter));

            counter++;

            if (counter >= v.Length)
            {
                counter = 0;
            }
        }
    }

    private static void SummonCells(CellData center, Faction faction)
    {
        MapGrid.BindCell(center, faction.Core);

        foreach (var cell in center.Neighbors)
        {
            MapGrid.BindCell(cell, faction.Core);
        }

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N).GetNeighbor(Direction.N), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E).GetNeighbor(Direction.E), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S).GetNeighbor(Direction.S), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W).GetNeighbor(Direction.W), "BindRune", faction);
    }
}