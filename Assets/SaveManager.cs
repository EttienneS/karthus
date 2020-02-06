using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    public static Save SaveToLoad { get; set; }

    public static void Restart(Save save = null)
    {
        Game.Instance = null;
        SaveToLoad = save;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public static void Load()
    {
        Game.TimeManager.Pause();

        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText("save.json"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        Restart(save);
    }

    public static void Save()
    {
        try
        {
            Game.TimeManager.Pause();

            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());

            var serializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };

            using (var sw = new StreamWriter("save.json"))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, MakeSave(), typeof(Save));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to save: {ex}");
        }
    }

    public static Save MakeSave()
    {
        return new Save
        {
            Seed = Game.Map.Seed,
            Factions = Game.FactionController.Factions.Values.ToList(),
            Time = Game.TimeManager.Data,
            Items = Game.IdService.ItemLookup.Values.ToList(),
            CameraData = new CameraData(Game.CameraController.Camera),
            Rooms = Game.ZoneController.RoomZones,
            Stores = Game.ZoneController.StorageZones,
            Areas = Game.ZoneController.AreaZones,
        };
    }
}