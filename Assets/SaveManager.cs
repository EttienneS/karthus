using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public void Restart()
    {
        Game.Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void Load()
    {
        Game.TimeManager.Pause();

        DestroyScene();

        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText("save.json"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        Game.TimeManager.Data = save.Time;

        Game.Map.ClearCache();
        Game.Map.Seed = save.Seed;

        Game.Instance.Ready = false;
        Game.MapGenerator.Done = false;

        foreach (var faction in save.Factions)
        {
            Game.FactionController.Factions.Add(faction.FactionName, faction);

            foreach (var creature in faction.Creatures.ToList())
            {
                Game.CreatureController.SpawnCreature(creature, creature.Cell, faction);
            }

            foreach (var structure in faction.Structures.ToList())
            {
                Game.IdService.EnrollEntity(structure);
            }
        }

        save.CameraData.Load(Game.CameraController.Camera);
    }

    public void Save()
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
                serializer.Serialize(writer, new Save(), typeof(Save));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to save: {ex}");
        }
    }

    private static void DestroyScene()
    {
        var creatures = Game.IdService.CreatureLookup.Values.ToList();
        foreach (var creature in creatures)
        {
            Game.CreatureController.DestroyCreature(creature.CreatureRenderer);
        }
        Game.Instance.DestroyItemsInCache();

        var structures = Game.IdService.StructureLookup.Values.ToList();
        foreach (var structure in structures)
        {
            Game.StructureController.DestroyStructure(structure);
        }
        Game.Instance.DestroyItemsInCache();

        foreach (var cell in Game.Map.Cells)
        {
            Game.Map.DestroyCell(cell);
        }
        Game.Instance.DestroyItemsInCache();

        Game.Map.CellLookup.Clear();
        Game.Map.Cells.Clear();

        Game.Map.Tilemap.ClearAllTiles();
        Game.StructureController.DefaultStructureMap.ClearAllTiles();

        Game.IdService.Clear();

        Game.FactionController.Factions.Clear();
    }
}