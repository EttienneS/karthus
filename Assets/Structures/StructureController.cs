using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    public Structure[] structurePrefabs;
    internal Dictionary<string, Structure> AllStructures = new Dictionary<string, Structure>();
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

    internal Structure GetStructure(Structure structurePrefab)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.name = name;

        return structure;
    }

    internal Structure GetStructureBluePrint(Structure structurePrefab)
    {
        var structure = GetStructure(structurePrefab);
        structure.BluePrint = true;
        return structure;
    }

    private void Awake()
    {
        foreach (var prefab in structurePrefabs)
        {
            var structure = Instantiate(prefab, transform);
            structure.name = prefab.name;
            AllStructures.Add(prefab.name, structure);
        }
    }
}