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
        Cells = MapGrid.Instance.Cells.Cast<Cell>().Select(c => c.Data).ToArray();
        //Creatures = CreatureController.Instance.Creatures.ToArray();
    }
}
