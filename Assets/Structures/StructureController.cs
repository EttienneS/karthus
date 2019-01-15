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

    internal Structure GetStructureBluePrint()
    {
        var structure = GetStructure();
        structure.BluePrint = true;
        return structure;
    }

    internal Structure GetStructure()
    {
        return Instantiate(structurePrefab, transform);
    }
}
