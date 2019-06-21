using System.Linq;

public partial class Game // .Spawn
{
    public static void SpawnRune(CellData location, string name, Faction faction)
    {
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
        midCell.CellType = CellType.Mountain;

        CreatureController.SpawnPlayerAtLocation(midCell.GetNeighbor(Direction.E));
        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        for (int i = 0; i < 3; i++)
        {
            var c = CreatureController.Beastiary.First().Value.CloneJson();
            c.Coordinates = MapGrid.GetRandomCell().Coordinates;
            CreatureController.SpawnCreature(c);

            FactionController.Factions[FactionConstants.Monster].AddCreature(c);
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

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.E), "Pylon", faction);
        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.W), "Pylon", faction);
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.E), "Pylon", faction);
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.W), "Pylon", faction);

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S), "BindRune", faction);
        SpawnRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W), "BindRune", faction);
    }
}