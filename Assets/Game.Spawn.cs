using System.Linq;

public partial class Game // .Spawn
{
    public static void SpawnRune(CellData location, string name)
    {
        if (location.Structure != null)
            StructureController.DestroyStructure(location.Structure);

        var rune = StructureController.GetStructure(name);
        location.CellType = CellType.Stone;
        MapGrid.BindCell(location, rune.Data.GetGameId());
        location.AddContent(rune.gameObject);
    }

    public static void InitialSpawn()
    {
        var midCell = MapGrid
            .GetCircle(new Coordinates(MapConstants.MapSize / 2, MapConstants.MapSize / 2), 10)
            .First(c => c.CellType != CellType.Water || c.CellType != CellType.Mountain);

        SummonCells(midCell);

        CreatureController.SpawnPlayerAtLocation(midCell);
        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        for (int i = 0; i < 3; i++)
        {
            var c = CreatureController.Beastiary.First().Value.CloneJson();
            c.Coordinates = MapGrid.GetRandomCell().Coordinates;
            c.Faction = FactionConstants.Monster;

            CreatureController.SpawnCreature(c);
        }
    }

    private static void SummonCells(CellData center)
    {
        center.CellType = CellType.Stone;
        MapGrid.BindCell(center, "X");

        foreach (var cell in center.Neighbors)
        {
            cell.CellType = CellType.Stone;
            MapGrid.BindCell(cell, "X");
        }

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.E), "Pylon");
        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.W), "Pylon");
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.E), "Pylon");
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.W), "Pylon");

        SpawnRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N), "BindRune");
        SpawnRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E), "BindRune");
        SpawnRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S), "BindRune");
        SpawnRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W), "BindRune");
    }
}