using Structures.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Structures
{
    public class StructureController : MonoBehaviour
    {
        public StructureRenderer StructureRendererPrefab;

        private static List<Type> _structureTypes;
        private float _lastUpdate;
        private Dictionary<string, Structure> _structureDataReference;
        private Dictionary<string, string> _structureTypeFileMap;

        public static List<Type> StructureTypes
        {
            get
            {
                return _structureTypes == null ? ReflectionHelper.GetAllTypes(typeof(Structure)) : _structureTypes;
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
                    foreach (var structureFile in Game.FileController.StructureJson)
                    {
                        try
                        {
                            var data = GetFromJson(structureFile.text);
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

        public static Structure GetFromJson(string json)
        {
            var structure = json.LoadJson<Structure>();
            var type = GetTypeFor(structure.Type);

            if (type != null)
            {
                return json.LoadJson(type) as Structure;
            }

            return structure;
        }

        public static Type GetTypeFor(string name)
        {
            return StructureTypes.Find(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void ClearStructure(Structure structure)
        {
            if (structure.IsFloor())
            {
                structure.Cell.RefreshTile();
            }
        }

        public void RefreshStructure(Structure structure)
        {
            if (structure.Cell == null)
            {
                return;
            }
            if (structure.IsFloor())
            {
                structure.Cell.RefreshTile();
            }
            else
            {
                structure.Renderer.SpriteRenderer.sprite = structure.GetSprite();
            }
        }

        public Structure SpawnStructure(string name, Cell cell, Faction faction, bool draw = true)
        {
            var structureData = StructureTypeFileMap[name];

            var structure = GetFromJson(structureData);
            var renderer = Instantiate(StructureRendererPrefab, transform);
            structure.Renderer = renderer;
            renderer.Data = structure;

            structure.Cell = cell;
            IndexStructure(structure);

            if (draw)
            {
                structure.Refresh();
            }
            faction?.AddStructure(structure);

            if (structure is Container container)
            {
                var zone = Game.ZoneController.GetZoneForCell(cell);

                if (zone != null && zone is StorageZone store)
                {
                    container.Filter = store.Filter;
                }
            }

            RefreshStructure(structure);

            return structure;
        }

        public void Update()
        {
            if (!Game.Instance.Ready)
                return;

            if (Game.TimeManager.Paused)
                return;

            _lastUpdate += Time.deltaTime;

            if (_lastUpdate > Game.TimeManager.CreatureTick)
            {
                _lastUpdate = 0;
                foreach (var structure in Game.IdService.StructureLookup.Values.OfType<WorkStructureBase>().Where(s => !s.IsBluePrint))
                {
                    structure.Process(Game.TimeManager.CreatureTick);
                }
            }
        }

        internal void DestroyStructure(Structure structure)
        {
            if (structure != null)
            {
                if (structure.Cell != null)
                {
                    ClearStructure(structure);
                }
                else
                {
                    Debug.Log("Unbound structure");
                }

                Game.IdService.RemoveEntity(structure);
                Game.FactionController.Factions[structure.FactionName].Structures.Remove(structure);
            }
        }

        internal Structure GetStructureBluePrint(string name, Cell cell, Faction faction)
        {
            var structure = SpawnStructure(name, cell, faction);
            structure.IsBluePrint = true;
            structure.Refresh();
            return structure;
        }

        private void IndexStructure(Structure structure)
        {
            Game.IdService.EnrollEntity(structure);
        }
    }
}