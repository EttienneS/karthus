using System.Linq;

public class Save
{
    public CameraData CameraData;
    public Cell[] Cells;

    public Faction[] Factions;

    public TimeData Time;

    public Save()
    {
        Cells = Game.Map.Cells.ToArray();
        Factions = FactionController.Factions.Values.ToArray();
        Time = Game.TimeManager.Data;
        CameraData = new CameraData(Game.CameraController.Camera);
    }
}
