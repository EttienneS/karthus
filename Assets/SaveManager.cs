using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
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

        Game.Map.Cells = save.Cells;
        Game.Map.ClearCache();

        Game.MapGenerator.LinkNeighbours();
        Game.MapGenerator.ResetSearchPriorities();

        foreach (var faction in save.Factions)
        {
            FactionController.Factions.Add(faction.FactionName, faction);

            foreach (var creature in faction.Creatures.ToList())
            {
                Game.CreatureController.SpawnCreature(creature, creature.Cell, faction);
            }

            foreach (var structure in faction.Structures.ToList())
            {
                IdService.EnrollEntity(structure);
            }
        }

        foreach (var cell in Game.Map.Cells.Where(c => c.Bound))
        {
            if (cell.DrawnOnce)
            {
                cell.UpdateTile();
            }
        }

        save.CameraData.Load(Game.CameraController.Camera);

    }

    public void Save()
    {
        try
        {
            Game.TimeManager.Pause();

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.Formatting = Formatting.Indented;

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
        var creatures = IdService.CreatureLookup.Values.ToList();
        foreach (var creature in creatures)
        {
            Game.CreatureController.DestroyCreature(creature.CreatureRenderer);
        }
        Game.Controller.DestroyItemsInCache();

        var structures = IdService.StructureLookup.Values.ToList();
        foreach (var structure in structures)
        {
            Game.StructureController.DestroyStructure(structure);
        }
        Game.Controller.DestroyItemsInCache();

        foreach (var cell in Game.Map.Cells)
        {
            Game.Map.DestroyCell(cell);
        }
        Game.Controller.DestroyItemsInCache();

        Game.Map.CellLookup.Clear();
        Game.Map.Cells.Clear();

        Game.Map.Tilemap.ClearAllTiles();
        Game.StructureController.DefaultStructureMap.ClearAllTiles();

        IdService.Clear();

        FactionController.Factions.Clear();
    }
}