using Newtonsoft.Json;

namespace Structures.Work
{
    public class Light : WorkStructureBase
    {
        [JsonIgnore]
        public VisualEffect LightEffect { get; set; }
        public float Radius { get; set; } = 3f;
        public float Intensity { get; set; } = 1f;

        public override void Update(float delta)
        {
            if (Game.Instance.TimeManager.Data.Hour < 6 || Game.Instance.TimeManager.Data.Hour > 18)
            {
                if (LightEffect == null)
                {
                    LightEffect = Game.Instance.VisualEffectController.SpawnLightEffect(this, Cell.Vector, ColorHex.GetColorFromHex(), Radius, Intensity, float.MaxValue);
                }
            }
            else
            {
                if (LightEffect != null)
                {
                    LightEffect.DestroySelf();
                }
                LightEffect = null;
            }
        }
    }
}