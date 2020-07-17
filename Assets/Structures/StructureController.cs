using Assets.Helpers;
using Structures.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Assets.Structures
{
    public class StructureController : MonoBehaviour
    {
        public GameObject RoofContainer;

        private static List<Type> _structureTypes;
        private float _lastUpdate;
        private Dictionary<string, Structure> _structureDataReference;
        private Dictionary<string, string> _structureTypeFileMap;

        public static List<Type> StructureTypes
        {
            get
            {
                if (_structureTypes == null)
                {
                    _structureTypes = ReflectionHelper.GetAllTypes(typeof(Structure));
                }

                return _structureTypes;
            }
        }

        internal Dictionary<string, Structure> StructureDataReference
        {
            get
            {
                StructureTypeFileMap.First();
                return _structureDataReference;
            }
        }

        internal Dictionary<string, string> StructureTypeFileMap
        {
            get
            {
                if (_structureTypeFileMap == null)
                {
                    _structureTypeFileMap = new Dictionary<string, string>();
                    _structureDataReference = new Dictionary<string, Structure>();
                    foreach (var structureFile in Game.Instance.FileController.StructureJson)
                    {
                        try
                        {
                            var data = LoadStructureFromJson(structureFile.text);
                            _structureTypeFileMap.Add(data.Name, structureFile.text);
                            _structureDataReference.Add(data.Name, data);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Unable to load structure {structureFile}: {ex.Message}");
                        }
                    }
                }
                return _structureTypeFileMap;
            }
        }

        public static Type GetTypeFor(string name)
        {
            return StructureTypes.Find(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static Structure LoadStructureFromJson(string json)
        {
            var structure = json.LoadJson<Structure>();
            var type = GetTypeFor(structure.Type);

            if (type != null)
            {
                return json.LoadJson(type) as Structure;
            }

            return structure;
        }

        public void CreateRoof(Cell cell)
        {
            var roof = Instantiate(Game.Instance.MeshRendererFactory.GetStructureMesh("Roof"), RoofContainer.transform);
            roof.transform.position = new Vector3(cell.X, cell.Y, cell.Z) + new Vector3(0.5f, 2.05f, 0.5f);
        }

        public Structure CreateNewStructure(string structureName)
        {
            return LoadStructureFromJson(StructureTypeFileMap[structureName]);
        }

        public MeshRenderer GetMeshForStructure(string name)
        {
            return GetMeshForStructure(_structureDataReference[name]);
        }

        public MeshRenderer GetMeshForStructure(Structure structure)
        {
            return Game.Instance.MeshRendererFactory.GetStructureMesh(structure.Mesh.Split(',').GetRandomItem());
        }

        public MeshRenderer InstantiateNewStructureMeshRenderer(string structureName, Transform parent)
        {
            return InstantiateNewStructureMeshRenderer(CreateNewStructure(structureName), parent);
        }

        public MeshRenderer InstantiateNewStructureMeshRenderer(Structure structure, Transform parent)
        {
            return InstantiateNewStructureMeshRenderer(GetMeshForStructure(structure), parent);
        }

        public MeshRenderer InstantiateNewStructureMeshRenderer(MeshRenderer mesh, Transform parent)
        {
            return Instantiate(mesh, parent);
        }

        public BlueprintRenderer SpawnBlueprint(string name, Cell cell, Faction faction)
        {
            var meshRenderer = InstantiateNewStructureMeshRenderer(name, transform);
            meshRenderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
            meshRenderer.transform.name = $"Blueprint: {name}";

            var blueprintRenderer = meshRenderer.gameObject.AddComponent<BlueprintRenderer>();
            var blueprint = new Blueprint(name, cell, faction);
            faction.AddBlueprint(blueprint);

            blueprintRenderer.Load(blueprint);
            return blueprintRenderer;
        }

        public Structure SpawnStructure(string name, Cell cell, Faction faction)
        {
            var structure = CreateNewStructure(name);
            var renderer = InstantiateNewStructureMeshRenderer(structure, transform);

            var structureRenderer = renderer.gameObject.AddComponent<StructureRenderer>();
            structure.Renderer = structureRenderer;
            structureRenderer.Data = structure;
            structureRenderer.Renderer = renderer;

            if (structure.SpawnRotation)
            {
                structure.Rotation = Random.Range(1, 360);
            }

            structure.Cell = cell;
            IndexStructure(structure);

            faction?.AddStructure(structure);

            if (structure is Container container)
            {
                var zone = Game.Instance.ZoneController.GetZoneForCell(cell);

                if (zone != null && zone is StorageZone store)
                {
                    container.Filter = store.Filter;
                }
            }

            structureRenderer.transform.name = structure.Name + " " + structure.Id;

            return structure;
        }

        public void Update()
        {
            if (Game.Instance.TimeManager.Paused)
                return;

            _lastUpdate += Time.deltaTime;

            if (_lastUpdate > Game.Instance.TimeManager.CreatureTick)
            {
                _lastUpdate = 0;
                foreach (var structure in Game.Instance.IdService.StructureLookup.Values.OfType<WorkStructureBase>())
                {
                    structure.Process(Game.Instance.TimeManager.CreatureTick);
                }
            }
        }

        internal void DestroyBlueprint(Blueprint blueprint)
        {
            if (blueprint.BlueprintRenderer != null)
            {
                Game.Instance.AddItemToDestroy(blueprint.BlueprintRenderer.transform.gameObject);

                foreach (var faction in Game.Instance.FactionController.Factions.Values)
                {
                    if (faction.Blueprints.Contains(blueprint))
                    {
                        faction.Blueprints.Remove(blueprint);
                        return;
                    }
                }
            }
        }

        internal void DestroyStructure(Structure structure)
        {
            if (structure != null)
            {
                if (structure.Cell == null)
                {
                    Debug.Log("Unbound structure");
                }

                Game.Instance.IdService.RemoveEntity(structure);
                Game.Instance.FactionController.Factions[structure.FactionName].Structures.Remove(structure);
                Game.Instance.AddItemToDestroy(structure.Renderer.gameObject);
            }
        }

        internal Cost GetStructureCost(string structureName)
        {
            return _structureDataReference[structureName].Cost;
        }

        private void IndexStructure(Structure structure)
        {
            Game.Instance.IdService.EnrollEntity(structure);
        }
    }
}