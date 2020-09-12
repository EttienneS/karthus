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

        public Wildfire(Structure structure) : base(structure)
        {
            RefreshSpreadDelta();
        }

        public override void Update(float delta)
        {
            if (_flameMesh == null)
            {
                InstantiateFlames();

                if (Structure.BaseFlammability > 150)
                {
                    Game.Instance.VisualEffectController.CreateFireLight(_flameMesh.transform, ColorExtensions.GetColorFromHex("F5810E"), 25, 25, 0.5f);
                }
            }

            SpreadDelta -= delta;
            if (SpreadDelta <= 0)
            {
                RefreshSpreadDelta();

                var open = GetFlammableNeighbours(Structure.Cell);

                if (open.Count > 0)
                {
                    var spreadStructure = open.GetRandomItem().Structure;
                    var meshName = spreadStructure.Mesh;
                    spreadStructure.AddBehaviour<Wildfire>();
                }
            }

            UpdateRemainingFlammability(delta);
        }

        private static List<Cell> GetFlammableNeighbours(Cell cell)
        {
            return Map.Instance.GetCircle(cell, 3)
                                   .Where(c => c.Structure != null && c.Structure.Flammable())
                                   .OrderByDescending(c => c.Structure.Flammability)
                                   .ToList();
        }

        private void InstantiateFlames()
        {
            _flameMesh = Game.Instance.MeshRendererFactory.CreateFlameMesh(Structure.Renderer.transform);
            var scale = Structure.BaseFlammability / 100f * 5f * Random.Range(0.75f, 1.25f);
            _flameMesh.transform.localScale = new Vector3(scale, scale, scale);
            _flameMesh.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        private void RefreshSpreadDelta()
        {
            SpreadDelta = Random.Range(25f, 50f) / (Structure.BaseFlammability / 10);
        }

        private void UpdateRemainingFlammability(float delta)
        {
            Structure.Flammability -= delta / 2;
            if (Structure.Flammability <= 0)
            {
                Game.Instance.StructureController.SpawnStructure("Debris", Structure.Cell, Game.Instance.FactionController.WorldFaction);
                Game.Instance.StructureController.DestroyStructure(Structure);
            }
        }
    }
}