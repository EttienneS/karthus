using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

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
        File.WriteAllText("save.json", JsonUtility.ToJson(new Save(), true));
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
    [SerializeField]
    public CellData[] Cells;
    //[SerializeField]
    //public Creature[] Creatures;

    public Save()
    {
        Cells = MapGrid.Instance.Cells.Select(c => c.Data).ToArray();
        //Creatures = CreatureController.Instance.Creatures.ToArray();
    }
}
