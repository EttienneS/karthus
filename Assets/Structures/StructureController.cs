using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    public Structure structurePrefab;

    private static StructureController _instance;

    public static StructureController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("StructureController").GetComponent<StructureController>();
            }

            return _instance;
        }
    }

    internal Structure GetStructureBluePrint(string name)
    {
        var structure = GetStructure(name);
        structure.BluePrint = true;
        return structure;
    }

    internal Structure GetStructure(string name)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.name = name;

        return structure;
    }
}
