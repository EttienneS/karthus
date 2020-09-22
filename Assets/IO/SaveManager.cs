using Assets.Map;
using Assets.ServiceLocator;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    private const string RootSaveDir = "Saves";

    public static string SaveDir
    {
        get
        {
            return $"{RootSaveDir}\\{Game.MapGenerationData.Seed}\\";
        }
    }

    public static Save SaveToLoad { get; set; }

    public static void Load(string saveFile)
    {
        Loc.GetTimeManager().Pause();

        if (string.IsNullOrEmpty(saveFile))
        {
            saveFile = Directory.EnumerateFiles(SaveDir).Last();
        }

        Restart(Save.FromFile(saveFile));
    }

 

    public static Save MakeSave()
    {
        return new Save
        {
            MapGenerationData = Game.MapGenerationData,
            Factions = Loc.GetFactionController().Factions.Values.ToList(),
            Time = Loc.GetTimeManager().Data,
            Items = Loc.GetIdService().ItemIdLookup.Values.ToList(),
            CameraData = new CameraData(Loc.GetCamera()),
            Rooms = Loc.GetZoneController().RoomZones,
            Stores = Loc.GetZoneController().StorageZones,
            Areas = Loc.GetZoneController().AreaZones,
            Chunks = Loc.GetMap().Chunks.Values.Select(s => s.Data).ToList(),
        };
    }

    public static void Restart(Save save = null)
    {
        Loc.Reset();
        SaveToLoad = save;
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public static void SaveGame()
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

    public static string GetLastSave()
    {
        var dir = new DirectoryInfo(RootSaveDir);
        var latest = dir.GetFiles("*.json", SearchOption.AllDirectories)
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();

        if (latest == null)
        {
            throw new FileNotFoundException("No save found!");
        }
        else
        {
            return latest.FullName;
        }
    }
}