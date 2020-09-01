using Assets.Item;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class Save
{
    public CameraData CameraData;

    public List<Faction> Factions;

    public List<ItemData> Items;

    public List<RoomZone> Rooms;
    public List<StorageZone> Stores;
    public List<AreaZone> Areas;
    public List<Chunk> Chunks;

    public TimeData Time;

    public string Seed;

    public static Save FromFile(string filename)
    {
        return JsonConvert.DeserializeObject<Save>(File.ReadAllText(filename), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }
}