using Assets.Creature.Behaviour;
using Assets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileController : MonoBehaviour
{
    public Dictionary<string, TextAsset> ItemIdLookup;
    public Material[] Materials;

    internal const string BiomeFolder = "Biomes";
    internal const string ConstructFolder = "Constructs";
    internal const string CreatureFolder = "Creatures";
    internal const string ItemsFolder = "Items";
    internal const string StructureFolder = "Structures";

    internal TextAsset[] BiomeFiles;
    internal TextAsset[] CreatureFiles;
    internal TextAsset[] ItemFiles;
    internal Dictionary<string, Material> MaterialLookup = new Dictionary<string, Material>();
    internal TextAsset[] StructureJson;
    private List<Type> _allBehaviours;
    private Material _blueprintMaterial;
    private List<Construct> _constructs;
    private Material _invalidBlueprintMaterial;
    private Material _suspendedBlueprintMaterial;
    public List<Type> AllBehaviourTypes
    {
        get
        {
            if (_allBehaviours == null)
            {
                _allBehaviours = ReflectionHelper.GetAllTypes(typeof(IBehaviour));
            }
            return _allBehaviours;
        }
    }

    public Material BlueprintMaterial
    {
        get
        {
            if (_blueprintMaterial == null)
            {
                _blueprintMaterial = GetMaterial("BlueprintMaterial");
            }
            return _blueprintMaterial;
        }
    }

    public List<Construct> Constructs
    {
        get
        {
            if (_constructs == null)
            {
                _constructs = new List<Construct>();
                foreach (var constructFile in Resources.LoadAll<TextAsset>(ConstructFolder))
                {
                    _constructs.Add(constructFile.text.LoadJson<Construct>());
                }
            }

            return _constructs;
        }
    }

    public Material InvalidBlueprintMaterial
    {
        get
        {
            if (_invalidBlueprintMaterial == null)
            {
                _invalidBlueprintMaterial = GetMaterial("InvalidBlueprintMaterial");
            }
            return _invalidBlueprintMaterial;
        }
    }

    public Material SuspendedBlueprintMaterial
    {
        get
        {
            if (_suspendedBlueprintMaterial == null)
            {
                _suspendedBlueprintMaterial = GetMaterial("SuspendedBlueprintMaterial");
            }
            return _suspendedBlueprintMaterial;
        }
    }
    public void Awake()
    {
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);
        BiomeFiles = Resources.LoadAll<TextAsset>(BiomeFolder);
        ItemFiles = Resources.LoadAll<TextAsset>(ItemsFolder);

        foreach (var material in Materials)
        {
            if (MaterialLookup.ContainsKey(material.name))
            {
                Debug.LogError($"Dupe material: {material.name}");
                continue;
            }
            MaterialLookup.Add(material.name, material);
        }
    }

    internal Material GetMaterial(string name)
    {
        if (MaterialLookup.ContainsKey(name))
        {
            return MaterialLookup[name];
        }
        return MaterialLookup["DefaultMaterial"];
    }

    internal Material[] GetMaterials(string materials)
    {
        if (string.IsNullOrEmpty(materials))
        {
            return null;
        }
        else
        {
            return materials.Split(',').Select(GetMaterial).ToArray();
        }
    }
}