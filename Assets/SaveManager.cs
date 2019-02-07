using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

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
            File.WriteAllText("save.json", JsonConvert.SerializeObject(new Save(), Formatting.Indented));

        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to save: {ex}");
        }
    }

    public void Load()
    {

    }

    private static void DestroyAndRecreateMap()
    {
        foreach (var cell in MapGrid.Instance.Cells)
        {
            Destroy(cell.gameObject);
        }

        foreach (var creature in CreatureController.Instance.Creatures)
        {
            Destroy(creature.gameObject);
        }



        CreatureController.Instance.Creatures.Clear();
        MapEditor.Instance.Generating = false;
    }
}

[Serializable]
public class Save
{
    public CellData[] Cells;


    public CreatureData[] Creatures;

    //
    //public TaskBase[] Tasks;

    public Save()
    {
        Cells = MapGrid.Instance.Cells.Select(c => c.Data).ToArray();
        Creatures = CreatureController.Instance.Creatures.Select(c => c.Data).ToArray();
        //  Tasks = Taskmaster.Instance.Tasks.ToArray();
    }
}
