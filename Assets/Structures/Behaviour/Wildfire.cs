using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Structures.Behaviour
{
    public class Wildfire : StructureBehaviour
    {
        [JsonIgnore]
        public const float MaxSpreadRate = 50f;

        [JsonIgnore]
        public const float MinSpreadRate = 15f;

        public float SpreadDelta;
        private MeshRenderer _flameMesh;

        public override void Update(float delta)
        {
            var structure = GetStructure();
            if (_flameMesh == null)
            {
                Initialize(structure);
            }

            SpreadDelta -= delta;
            if (SpreadDelta <= 0)
            {
                RefreshSpreadDelta(structure);

                var open = GetFlammableNeighbours(structure.Cell);

                if (open.Count > 0)
                {
                    var spreadStructure = open.GetRandomItem().Structures.GetRandomItem();
                    var meshName = spreadStructure.Mesh;
                    spreadStructure.AddBehaviour<Wildfire>();
                }
            }

            UpdateRemainingFlammability(structure, delta);
        }

        private void Initialize(Structure structure)
        {
            RefreshSpreadDelta(structure);

            InstantiateFlames(structure);

            if (structure.BaseFlammability > 150)
            {
                Game.Instance.VisualEffectController.CreateFireLight(_flameMesh.transform, ColorExtensions.GetColorFromHex("F5810E"), 25, 25, 0.5f);
            }
        }

        private static List<Cell> GetFlammableNeighbours(Cell cell)
        {
            return MapController.Instance.GetCircle(cell, 3)
                                   .Where(c => c.Structures.Any(s => s.Flammable()))
                                   .ToList();
        }

        private void InstantiateFlames(Structure structure)
        {
            _flameMesh = Game.Instance.MeshRendererFactory.CreateFlameMesh(structure.Renderer.transform);
            var scale = structure.BaseFlammability / 100f * 5f * Random.Range(0.75f, 1.25f);
            _flameMesh.transform.localScale = new Vector3(scale, scale, scale);
            _flameMesh.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        private void RefreshSpreadDelta(Structure structure)
        {
            SpreadDelta = Random.Range(25f, 50f) / (structure.BaseFlammability / 10);
        }

        private void UpdateRemainingFlammability(Structure structure, float delta)
        {
            structure.Flammability -= delta / 2;
            if (structure.Flammability <= 0)
            {
                Game.Instance.StructureController.SpawnStructure("Debris", structure.Cell, Game.Instance.FactionController.WorldFaction);
                Game.Instance.StructureController.DestroyStructure(structure);
            }
        }
    }
}