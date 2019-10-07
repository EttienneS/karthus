public class Spawn : EffectBase
{
    public Spawn()
    {
    }

    public override bool DoEffect()
    {
        if (SubTasksComplete())
        {
            Game.CreatureController
                .CacheSpawn(Game.CreatureController.GetCreatureOfType("Person"),
                               AssignedEntity.Cell,
                               AssignedEntity.GetFaction());

            Game.StructureController.DestroyStructure(Structure);
            return true;
        }

        return false;
    }

}