using Newtonsoft.Json;

public class MaintainVisualEffect : EffectBase
{
    [JsonIgnore]
    public VisualEffectData VisualEffect;

    public string Color;

    public bool Fades;
    public int Intensity = 1;
    public int Radius = 2;
    public int Duration = 2;

    public override bool DoEffect()
    {
        if (VisualEffect?.Destroyed != false)
        {
            if (VisualEffect != null && AssignedEntity.LinkedVisualEffects.Contains(VisualEffect))
            {
                AssignedEntity.LinkedVisualEffects.Remove(VisualEffect);
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
                VisualEffect = VisualEffect.LinkedGameObject.Fades().Data;
            }

        }

        return true;
    }
}