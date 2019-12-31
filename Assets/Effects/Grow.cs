using UnityEngine;

public class Grow : EffectBase
{
    public float Age;
    public int Stage;
    public int TotalStages;
    public float AgePerStage;
    public string PlantName;

    private VisualEffect _visualEffect;

    public override bool DoEffect()
    {
        if (AssignedEntity.Cell.Structure != null)
        {
            return true;
        }

        Age += Time.deltaTime;


        if (_visualEffect == null)
        {
            _visualEffect = Game.VisualEffectController.SpawnSpriteEffect(AssignedEntity, AssignedEntity.Cell.Vector, $"{PlantName}_{Stage}", float.MaxValue);
        }

        if (Age > AgePerStage)
        {
            Age = 0;
            Stage++;

            _visualEffect.DestroySelf();
            if (Stage >= TotalStages)
            {
                var structure = Game.StructureController.SpawnStructure(PlantName, AssignedEntity.Cell, AssignedEntity.GetFaction());
                Stage = 0;
                return true;
            }
            else
            {
                _visualEffect = Game.VisualEffectController.SpawnSpriteEffect(AssignedEntity, AssignedEntity.Cell.Vector, $"{PlantName}_{Stage}", float.MaxValue);
            }
        }

        return false;
    }
}