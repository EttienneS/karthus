using System.Collections.Generic;
using System.Linq;

public class Save
{
    public CameraData CameraData;
    public List<Cell> Cells;

    public List<Faction> Factions;

    public TimeData Time;

    public Save()
    {
        Cells = Game.Map.Cells;
        Factions = FactionController.Factions.Values.ToList();
        Time = Game.TimeManager.Data;
        CameraData = new CameraData(Game.CameraController.Camera);
    }
}
