using Newtonsoft.Json;

public class MaintainVisualEffect : EffectBase
{
    [JsonIgnore]
    public VisualEffectData VisualEffect;

    public override bool DoEffect()
    {
        if (VisualEffect?.Destroyed != false)
        {
            if (VisualEffect != null && AssignedEntity.LinkedVisualEffects.Contains(VisualEffect))
            {
                AssignedEntity.LinkedVisualEffects.Remove(VisualEffect);
                VisualEffect.LinkedGameObject.DestroySelf();
            }
            VisualEffect = Game.VisualEffectController
                               .SpawnLightEffect(AssignedEntity,
                                                 AssignedEntity.Cell,
                                                 AssignedEntity.ManaPool
                                                                .GetManaWithMost()
                                                                .GetActualColor(),
                                                 1, 2, 5)
                               .Fades()
                               .Data;
        }

        return true;
    }
}