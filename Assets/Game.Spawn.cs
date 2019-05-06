using System.Linq;

public partial class Game // .Spawn
{
    public static void GetRune(CellData location)
    {
        var rune = StructureController.GetStructure("BindRune");
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

        CreatureController.SpawnPlayerAtLocation(midCell.GetNeighbor(Direction.E));

        midCell.AddContent(StructureController.GetStructure("Table").gameObject);

        CameraController.MoveToCell(midCell.GetNeighbor(Direction.E));

        var spawns = midCell.Neighbors.ToList();

        var waterCell = midCell.GetNeighbor(Direction.N);
        waterCell.CellType = CellType.Water;

        var foodCell = midCell.GetNeighbor(Direction.SE);
        for (var i = 0; i < 30; i++)
        {
            foodCell.AddContent(ItemController.GetItem("Apple").gameObject);
        }

        var woodCell = midCell.GetNeighbor(Direction.SW);
        for (var i = 0; i < 15; i++)
        {
            woodCell.AddContent(ItemController.GetItem("Rock").gameObject);
        }

        var rockCell = midCell.GetNeighbor(Direction.S);
        for (var i = 0; i < 15; i++)
        {
            rockCell.AddContent(ItemController.GetItem("Wood").gameObject);
        }

        for (int i = 0; i < 3; i++)
        {
            var c = CreatureController.Beastiary.First().Value.CloneJson();
            c.Coordinates = MapGrid.GetRandomCell().Coordinates;
            c.Faction = FactionConstants.MonsterFaction;

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

        GetRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N));
        GetRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E));
        GetRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S));
        GetRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W));
    }
}