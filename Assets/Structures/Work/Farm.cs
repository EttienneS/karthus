using Assets.Structures;
using Newtonsoft.Json;
using UnityEngine;

namespace Structures.Work
{
    public class Farm : WorkStructureBase
    {
        [JsonIgnore]
        public const float MaxGrowth = 100f;

        [JsonIgnore]
        public VisualEffect PlantEffect;

        [JsonIgnore]
        private float _lastGrowthIndex = -1;
        public float CurrentGrowth { get; set; }

        [JsonIgnore]
        public float GrowthIndex
        {
            get
            {
                return (int)(CurrentGrowth / 25);
            }
        }

        public string PlantName { get; set; }
        public float Quality { get; set; }
        public string Sprite { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(PlantName))
            {
                return "\n** No crop selected. Choose one of the options and click the Select button. **\n";
            }

            return "Farm:\n" +
                   $"  Growing: {PlantName}\n" +
                   $"  Quality: {Quality:0,0}\n" +
                   $"  Grown: {(CurrentGrowth / MaxGrowth) * 100:0,0}%\n";
        }

        public override void Update(float delta)
        {
            if (!string.IsNullOrEmpty(PlantName))
            {
                CurrentGrowth += delta / 10;
                Quality += Random.Range(0, 0.001f);
            }
            else if (AutoOrder != null)
            {
                AutoCooldown = 0.01f;
            }

            if (!string.IsNullOrEmpty(PlantName))
            {
                if (PlantEffect == null)
                {
                    _lastGrowthIndex = GrowthIndex;
                    PlantEffect = Game.Instance.VisualEffectController.SpawnSpriteEffect(this, Vector, Sprite + "_" + _lastGrowthIndex, 10);
                }
                else if (GrowthIndex != _lastGrowthIndex)
                {
                    _lastGrowthIndex = GrowthIndex;
                    PlantEffect.Sprite.sprite = Game.Instance.SpriteStore.GetSprite(Sprite + "_" + _lastGrowthIndex);
                }

                if (CurrentGrowth >= MaxGrowth)
                {
                    Game.Instance.ItemController.SpawnItem(PlantName, Cell, (int)Quality);
                    CurrentGrowth = 0;
                }
            }
        }
    }
}