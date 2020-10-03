using Assets.Helpers;
using Assets.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Structures
{
    public class StructureController : MonoBehaviour, IGameService
    {
        public GameObject RoofContainer;

        internal Blueprint GetBlueprintById(string blueprintId)
        {
            return Loc.GetFactionController().Factions
                                .SelectMany(f => f.Value.Blueprints)
                                .FirstOrDefault(b => b.ID == blueprintId);
        }

        private float _lastUpdate;

        public List<Type> StructureTypes { get; set; }

        internal Dictionary<string, Structure> StructureDataReference { get; set; }

        internal Dictionary<string, string> StructureTypeFileMap { get; set; }

        public Type GetTypeFor(string name)
        {
            return StructureTypes.Find(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public Structure LoadStructureFromJson(string json)
        {
            var structure = json.LoadJson<Structure>();
            var type = GetTypeFor(structure.Type);

            if (type != null)
            {
                structure = json.LoadJson(type) as Structure;
            }

            return structure;
        }

        public Structure CreateNewStructure(string structureName)
        {
            return LoadStructureFromJson(StructureTypeFileMap[structureName]);
        }

        public void CreateRoof(Cell cell)
        {
            var roof = Instantiate(Loc.GetGameController().MeshRendererFactory.GetStructureMesh("Roof"), RoofContainer.transform);
            roof.transform.position = new Vector3(cell.X, cell.Y, cell.Z) + new Vector3(0.5f, 2.05f, 0.5f);
        }

        public MeshRenderer GetMeshForStructure(string name)
        {
            return GetMeshForStructure(StructureDataReference[name]);
        }

        public MeshRenderer GetMeshForStructure(Structure structure)
        {
            return Loc.GetGameController().MeshRendererFactory.GetStructureMesh(structure.Mesh.Split(',').GetRandomItem());
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
            var blueprint = new Blueprint(name, cell, faction);
            faction.AddBlueprint(blueprint);

            return SpawnBlueprint(blueprint);
        }

        public BlueprintRenderer SpawnBlueprint(Blueprint blueprint)
        {
            var meshRenderer = InstantiateNewStructureMeshRenderer(blueprint.StructureName, transform);
            meshRenderer.SetAllMaterial(Loc.GetFileController().BlueprintMaterial);
            meshRenderer.transform.name = $"Blueprint: {blueprint.StructureName}";

            var blueprintRenderer = meshRenderer.gameObject.AddComponent<BlueprintRenderer>();
            blueprintRenderer.Load(blueprint);
            return blueprintRenderer;
        }

        public Structure SpawnStructure(string name, Cell cell, Faction faction)
        {
            var structure = CreateNewStructure(name);
            structure.Cell = cell;
            faction?.AddStructure(structure);

            structure.Load();

            return SpawnStructure(structure);
        }

        public Structure SpawnStructure(Structure structure)
        {
            var chunk = Loc.GetMap().GetChunkForCell(structure.Cell);
            var renderer = InstantiateNewStructureMeshRenderer(structure, chunk.transform);
            //var renderer = InstantiateNewStructureMeshRenderer(structure, transform);

            var structureRenderer = renderer.gameObject.AddComponent<StructureRenderer>();
            structure.Renderer = structureRenderer;
            structureRenderer.Data = structure;
            structureRenderer.Renderer = renderer;

            IndexStructure(structure);

            structureRenderer.transform.name = structure.Name + " " + structure.Id;
            structureRenderer.UpdatePosition(chunk);

            if (structure is WorkStructureBase workStructure)
            {
                workStructure.Initialize();
                workStructure.PlaceDefaultOrder();
            }

            return structure;
        }

        public void Update()
        {
            if (Loc.GetTimeManager().Paused)
                return;

            UpdateWorkStructures();
        }

        internal void DestroyBlueprint(Blueprint blueprint)
        {
            if (blueprint.BlueprintRenderer != null)
            {
                Loc.GetGameController().AddItemToDestroy(blueprint.BlueprintRenderer.transform.gameObject);

                foreach (var faction in Loc.GetFactionController().Factions.Values)
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

                structure.OnDestroy();
                Loc.GetIdService().RemoveStructure(structure);
                Loc.GetFactionController().Factions[structure.FactionName].Structures.Remove(structure);
                Loc.GetGameController().AddItemToDestroy(structure.Renderer.gameObject);
            }
        }

        internal Cost GetStructureCost(string structureName)
        {
            return StructureDataReference[structureName].Cost;
        }

        private void IndexStructure(Structure structure)
        {
            Loc.GetIdService().EnrollStructure(structure);
        }

        private void UpdateWorkStructures()
        {
            _lastUpdate += Time.deltaTime;

            if (_lastUpdate > 0.5f)
            {
                foreach (var structure in Loc.GetIdService().StructureIdLookup.Values.OfType<WorkStructureBase>())
                {
                    structure.Process(_lastUpdate);
                }
                _lastUpdate = 0;
            }
        }

        public void Initialize()
        {
            StructureTypes = ReflectionHelper.GetAllTypes(typeof(Structure));

            StructureTypeFileMap = new Dictionary<string, string>();
            StructureDataReference = new Dictionary<string, Structure>();
            foreach (var structureFile in Loc.GetFileController().StructureJson)
            {
                try
                {
                    var data = LoadStructureFromJson(structureFile.text);
                    StructureTypeFileMap.Add(data.Name, structureFile.text);
                    StructureDataReference.Add(data.Name, data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unable to load structure {structureFile}: {ex.Message}");
                }
            }
        }
    }
}