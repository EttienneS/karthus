using Newtonsoft.Json;

public class MaintainVisualEffect : EffectBase
{
    [JsonIgnore]
    public VisualEffectData VisualEffect;

    public string Color;

    public bool Fades;
    public float Intensity = 1.0f;
    public float Radius = 2.0f;
    public float Duration = 2.0f;

    public override bool DoEffect()
    {
        if (VisualEffect?.Destroyed != false)
        {
            if (VisualEffect != null)
            {
                VisualEffect.LinkedGameObject.DestroySelf();
            }

            var color = string.IsNullOrEmpty(Color) ?
                                    AssignedEntity.ManaPool.GetManaWithMost().GetActualColor() :
                                    Color.GetColorFromHex();

            VisualEffect = Game.VisualEffectController
                               .SpawnLightEffect(AssignedEntity,
                                                 AssignedEntity.Cell,
                                                 color,
                                                 Radius, Intensity, Duration)
                               .Data;

            if (Fades)
            {
                VisualEffect.LinkedGameObject.Fades();
            }

        }

        return true;
    }
}