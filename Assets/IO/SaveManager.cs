using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    public static string SaveDir
    {
        get
        {
            return $"Saves\\{Game.Instance.MapData.Seed}\\";
        }
    }

    public static Save SaveToLoad { get; set; }

    public static void Load(string saveFile)
    {
        Game.Instance.TimeManager.Pause();

        if (string.IsNullOrEmpty(saveFile))
        {
            saveFile = Directory.EnumerateFiles(SaveDir).Last();
        }

        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(saveFile), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        Restart(save);
    }

    public static Save MakeSave()
    {
        return new Save
        {
            Seed = Game.Instance.MapData.Seed,
            Factions = Game.Instance.FactionController.Factions.Values.ToList(),
            Time = Game.Instance.TimeManager.Data,
            Items = Game.Instance.IdService.ItemLookup.Values.ToList(),
            CameraData = new CameraData(Game.Instance.CameraController.Camera),
            Rooms = Game.Instance.ZoneController.RoomZones,
            Stores = Game.Instance.ZoneController.StorageZones,
            Areas = Game.Instance.ZoneController.AreaZones,
            Chunks = Game.Instance.Map.Chunks.Values.Select(s => s.Data).ToList(),
        };
    }

    public static void Restart(Save save = null)
    {
        Game.Instance = null;
        SaveToLoad = save;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public static void Save()
    {
        try
        {
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
                Formatting = Formatting.Indented
            };

            Directory.CreateDirectory(SaveDir);

            var file = $"{SaveDir}\\{DateTime.Now:yy-MM-dd_HH-mm-ss}";
            ScreenCapture.CaptureScreenshot($"{file}.png");
            using (var sw = new StreamWriter($"{file}.json"))
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, MakeSave(), typeof(Save));
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to save: {ex}");
        }
    }

   
}