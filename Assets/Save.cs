using System.Collections.Generic;
using System.Linq;

public class Save
{
    public CameraData CameraData;

    public List<Faction> Factions;

    public List<Item> Items;

    public TimeData Time;

    public float Seed;

    public Save()
    {
        Seed = Game.Map.Seed;
        Factions = Game.FactionController.Factions.Values.ToList();
        Time = Game.TimeManager.Data;
        Items = Game.IdService.ItemLookup.Values.ToList();
        CameraData = new CameraData(Game.CameraController.Camera);
    }
}