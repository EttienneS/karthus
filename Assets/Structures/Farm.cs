using Assets.ServiceLocator;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Structures
{
    public class Farm : WorkStructureBase
    {
        private readonly float _growthRateDivisor = 10f;
        private readonly float _growthWhenMature = 100f;
        private readonly int _maxGrowthPhases = 4;

        private int _lastGrowthIndex = -1;
        private MeshRenderer _plantMesh;

        public float CurrentGrowth { get; set; }

        [JsonIgnore]
        public int GrowthIndex
        {
            get
            {
                return (int)(CurrentGrowth / 25);
            }
        }

        public string PlantName { get; set; }
        public float Quality { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(PlantName))
            {
                return "\n** No crop selected. Choose one of the options and click the Select button. **\n";
            }

            if (IsMature())
            {
                return "\n** Ready to harvest! **\n";
            }

            return "Farm:\n" +
                   $"  Growing: {PlantName}\n" +
                   $"  Quality: {Quality:0,0}\n" +
                   $"  Grown: {CurrentGrowth / _growthWhenMature * 100:0,0}%\n";
        }

        public override void Update(float delta)
        {
            if (!string.IsNullOrEmpty(PlantName))
            {
                ProcessPlantGrowth(delta);
            }
            else if (AutoOrder != null)
            {
                AutoCooldown = 0.01f;
            }
        }

        public void UpdatePlantMesh()
        {
            if (_lastGrowthIndex < 0 || _lastGrowthIndex != GrowthIndex)
            {
                _lastGrowthIndex = GrowthIndex;

                if (_lastGrowthIndex < _maxGrowthPhases)
                {
                    DestroyPlantMesh();
                    InstantiateNewPlantMesh();
                    SetPlantMeshRotation();
                }
            }
        }

        internal bool IsMature()
        {
            return CurrentGrowth >= _growthWhenMature;
        }

        internal void ResetGrowth()
        {
            CurrentGrowth = 0;
            Quality = 0;
        }

        internal void SpawnYield()
        {
            Loc.GetItemController().SpawnItem(PlantName, Cell, (int)Quality);
        }

        private void DestroyPlantMesh()
        {
            if (_plantMesh != null)
            {
                Renderer.DestroySubMesh(_plantMesh);
            }
        }

        private float GetTimeBasedQualityChange()
        {
            // small change not based of tend quality
            // also used when plant is mature to slowly decline quality
            return Random.Range(0.001f, 0.0025f);
        }

        private void InstantiateNewPlantMesh()
        {
            var meshName = PlantName + "_" + (_lastGrowthIndex + 1);
            var mesh = Loc.GetGameController().MeshRendererFactory.GetItemMesh(meshName);
            _plantMesh = Renderer.InstantiateSubMesh(mesh);
        }

        private void ProcessPlantGrowth(float delta)
        {
            if (IsMature())
            {
                // done growing wait for harvest
                Quality -= GetTimeBasedQualityChange();

                if (Quality < 1)
                {
                    ClearPlant();
                }
            }
            else
            {
                CurrentGrowth += delta / _growthRateDivisor;
                Quality += GetTimeBasedQualityChange();
                UpdatePlantMesh();
            }
        }

        private void ClearPlant()
        {
            ResetGrowth();
            DestroyPlantMesh();
            PlantName = null;
        }

        private void SetPlantMeshRotation()
        {
            _plantMesh.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
        }

        public override void Initialize()
        {
        }
    }
}