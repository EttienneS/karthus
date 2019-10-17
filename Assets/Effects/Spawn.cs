public class Spawn : EffectBase
{
    public Spawn()
    {
    }

    public override bool DoEffect()
    {
        Game.CreatureController
                .CacheSpawn(Game.CreatureController.GetCreatureOfType("Person"),
                               AssignedEntity.Cell,
                               AssignedEntity.GetFaction());

        IdService.DestroyEntity(AssignedEntity);
        return true;
    }
}