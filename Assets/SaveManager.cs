using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            }

            return _instance;
        }
    }

    public void Save()
    {
        try
        {
            TimeManager.Instance.Pause();

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
        TimeManager.Instance.Pause();

        DestroyScene();

        var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText("save.json"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        TimeManager.Instance.Data = save.Time;

        foreach (var saveCell in save.Cells)
        {
            var newCell = MapGrid.Instance.CreateCell(saveCell.Coordinates.X, saveCell.Coordinates.Y, saveCell.CellType);
            newCell = saveCell;


            if (saveCell.Structure != null)
            {
                newCell.AddContent(StructureController.Instance.LoadStructure(saveCell.Structure).gameObject);
            }

            if (saveCell.Stockpile != null)
            {
                newCell.AddContent(StockpileController.Instance.LoadStockpile(saveCell.Stockpile).gameObject);
            }

            // ensure we do not add duplicates
            var savedItems = saveCell.ContainedItems.ToArray();
            newCell.ContainedItems.Clear();

            foreach (var savedItem in savedItems)
            {
                newCell.AddContent(ItemController.Instance.LoadItem(savedItem).gameObject);
            }
        }

        MapGrid.Instance.ClearCache();
        MapGrid.Instance.LinkNeighbours();
        MapGrid.Instance.ResetSearchPriorities();

        foreach (var SavedCreature in save.Creatures)
        {
            CreatureController.Instance.LoadCreature(SavedCreature);
        }

        foreach (var task in save.Tasks)
        {
            Taskmaster.Instance.AddTask(task, task.Originator);

            if (task.CreatureId > 0)
            {
                CreatureController.Instance.CreatureIdLookup[task.CreatureId].LinkedGameObject.AssignTask(task);
            }
        }

        save.CameraData.Load(CameraController.Instance.Camera);
    }

    private static void DestroyScene()
    {
        foreach (var cell in MapGrid.Instance.Cells)
        {
            MapGrid.Instance.DestroyCell(cell);
        }

        foreach (var creature in CreatureController.Instance.Creatures)
        {
            CreatureController.Instance.DestroyCreature(creature);
        }

        MapGrid.Instance.CellLookup.Clear();
        MapGrid.Instance.Cells.Clear();

        CreatureController.Instance.Creatures.Clear();
        CreatureController.Instance.CreatureLookup.Clear();
        CreatureController.Instance.CreatureIdLookup.Clear();

        ItemController.Instance.ItemTypeIndex.Clear();
        ItemController.Instance.ItemDataLookup.Clear();
        ItemController.Instance.ItemIdLookup.Clear();

        StructureController.Instance.StructureLookup.Clear();

        Taskmaster.Instance.Tasks.Clear();
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
        Cells = MapGrid.Instance.Cells.ToArray();
        Creatures = CreatureController.Instance.Creatures.Select(c => c.Data).ToArray();
        Tasks = Taskmaster.Instance.Tasks.ToArray();
        Time = TimeManager.Instance.Data;
        CameraData = new CameraData(CameraController.Instance.Camera);
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