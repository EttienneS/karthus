using Newtonsoft.Json;

public class ClearCell : TaskBase
{
    [JsonIgnore]
    private CellData _cellData;

    [JsonIgnore]
    public CellData Cell
    {
        get
        {
            if (_cellData == null)
            {
                _cellData = Game.MapGrid.GetCellAtCoordinate(Coordinates);
            }

            return _cellData;
        }
    }

    public Coordinates Coordinates;

    public ClearCell()
    {
    }

    public ClearCell(Coordinates coordinates)
    {
        Coordinates = coordinates;
        Message = $"Clearing cell at {coordinates}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Cell.ContainedItems.Count > 0)
            {
                AddSubTask(new MoveItemToCell(Cell.ContainedItems[0].GetGameId(),
                                              Game.MapGrid.GetPathableNeighbour(Coordinates),
                                              true,
                                              true,
                                              GetItem.SearchBy.Id));
            }
            else
            {
                return true;
            }
        }

        return false;
    }
}