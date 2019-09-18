public class Spawn : EntityTask
{
    public Spawn()
    {
    }

    public override bool Done()
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