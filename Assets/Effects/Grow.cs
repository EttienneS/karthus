public class Grow : EffectBase
{
    public int Stage = -1;
    public int TotalStages;
    public string PlantName;

    private VisualEffect _visualEffect;

    public override bool DoEffect()
    {
        if (AssignedEntity.Cell.Structure != null)
        {
            return true;
        }

        Stage++;

        if (Stage >= TotalStages)
        {
            var structure = Game.StructureController.SpawnStructure(PlantName, AssignedEntity.Cell, AssignedEntity.GetFaction());
            structure.Refresh();
            Stage = 0;
            return true;
        }
        else
        {
            _visualEffect?.DestroySelf();
            _visualEffect = Game.VisualEffectController.SpawnSpriteEffect(AssignedEntity, AssignedEntity.Cell.Vector, $"{PlantName}_{Stage}", float.MaxValue);
        }

        return false;
    }
}