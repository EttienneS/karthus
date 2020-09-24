using Assets.Item;
using Assets.Map;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    public MapGenerationData MapGenerationData;

    public static Save FromFile(string filename)
    {
        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(filename), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        return save;
    }
}