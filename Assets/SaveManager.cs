using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
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

        foreach (var saveCell in save.Cells)
        {
            var newCell = Game.MapGrid.CreateCell(saveCell.Coordinates.X, saveCell.Coordinates.Y, saveCell.CellType);
            newCell = saveCell;

            if (saveCell.Structure != null)
            {
                newCell.AddContent(Game.StructureController.LoadStructure(saveCell.Structure).gameObject);
            }
        }

        Game.MapGrid.ClearCache();
        Game.MapGrid.LinkNeighbours();
        Game.MapGrid.ResetSearchPriorities();

        foreach (var SavedCreature in save.Creatures)
        {
            Game.CreatureController.SpawnCreature(SavedCreature);
        }

        //foreach (var task in save.Tasks)
        //{
        //    Factions.Taskmasters[Data.Faction].AddTask(task, task.Originator);

        //    if (task.AssignedCreatureId > 0)
        //    {
        //        Taskmaster.AssignTask(Game.CreatureController.CreatureIdLookup[task.AssignedCreatureId], task, task.Context);
        //    }
        //}

        save.CameraData.Load(Game.CameraController.Camera);
    }

    private static void DestroyScene()
    {
        foreach (var cell in Game.MapGrid.Cells)
        {
            Game.MapGrid.DestroyCell(cell);
        }

        foreach (var creature in Game.CreatureController.Creatures)
        {
            Game.CreatureController.DestroyCreature(creature);
        }

        Game.MapGrid.CellLookup.Clear();
        Game.MapGrid.Cells.Clear();

        Game.CreatureController.Creatures.Clear();
        Game.CreatureController.CreatureLookup.Clear();
        Game.CreatureController.CreatureIdLookup.Clear();

        Game.StructureController.StructureLookup.Clear();

        //Factions.Taskmasters[Data.Faction].Tasks.Clear();
    }
}

public class Save
{
    public CellData[] Cells;

    public CreatureData[] Creatures;

    public TaskBase[] Tasks;

    public TimeData Time;

    public CameraData CameraData;

    public Save()
    {
        Cells = Game.MapGrid.Cells.ToArray();
        Creatures = Game.CreatureController.Creatures.Select(c => c.Data).ToArray();
        //Tasks = Factions.Taskmasters[Data.Faction].Tasks.ToArray();
        Time = Game.TimeManager.Data;
        CameraData = new CameraData(Game.CameraController.Camera);
    }
}

public class CameraData
{
    public float X;
    public float Y;
    public float Z;

    public float Zoom;

    public CameraData()
    {
    }

    public CameraData(Camera c)
    {
        X = c.transform.position.x;
        Y = c.transform.position.y;
        Z = c.transform.position.z;

        Zoom = c.orthographicSize;
    }

    public void Load(Camera c)
    {
        c.transform.position = new Vector3(X, Y, Z);
        c.orthographicSize = Zoom;
    }
}