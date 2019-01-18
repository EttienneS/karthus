using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    public Structure structurePrefab;
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

    private void Start()
    {
        foreach (var structureFile in FileController.Instance.LoadJsonFilesInFolder("Structures"))
        {
            var structure = Instantiate(structurePrefab, transform);

            structure.Load(FileController.Instance.GetFile(structureFile));
            structure.name = structure.StructureData.Name;

            AllStructures.Add(structure.StructureData.Name, structure);
        }
    }
}