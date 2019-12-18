using Newtonsoft.Json;

public class MaintainVisualEffect : EffectBase
{
    [JsonIgnore]
    public VisualEffectData VisualEffect;

    public string Color;

    public bool Fades;
    public float Intensity;
    public float Radius;
    public float Duration;

    public override bool DoEffect()
    {
        if (VisualEffect?.Destroyed != false)
        {
            //if (VisualEffect != null)
            //{
            //    VisualEffect.LinkedGameObject.DestroySelf();
            //}

            //var color = string.IsNullOrEmpty(Color) ?
            //                        AssignedEntity.ManaValue.GetManaWithMost().GetActualColor() :
            //                        Color.GetColorFromHex();

            //VisualEffect = Game.VisualEffectController
            //                   .SpawnLightEffect(AssignedEntity,
            //                                     AssignedEntity.Cell.Vector,
            //                                     color,
            //                                     Radius, Intensity, Duration)
            //                   .Data;

            //if (Fades)
            //{
            //    VisualEffect.LinkedGameObject.Fades();
            //}
        }

        return true;
    }
}