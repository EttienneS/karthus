using System.Linq;
using Structures;
public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;

    public RemoveStructure()
    {
        RequiredSkill = SkillConstants.Build;
        RequiredSkillLevel = 1;
    }

    public RemoveStructure(Structure structure) : this()
    {
        StructureToRemove = structure;

        AddSubTask(new Move(Game.Map.GetPathableNeighbour(StructureToRemove.Cell)));
        AddSubTask(new Wait(structure.Cost.Items.Sum(c => c.Value) + structure.Cost.Mana.Sum(c => c.Value), "De-constructing..."));

        Message = $"Removing {StructureToRemove.Name} at {StructureToRemove.Cell}";
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            foreach (var item in StructureToRemove.Cost.Items)
            {
                var spawnedItem = Game.ItemController.SpawnItem(item.Key, StructureToRemove.Cell);
                spawnedItem.Amount = item.Value;
                spawnedItem.FactionName = creature.FactionName;

                // claim the entity to ensure that it can be used even if outside the 'home' area
                creature.ClaimEntityForFaction(spawnedItem);
            }

            Game.StructureController.DestroyStructure(StructureToRemove);

            return true;
        }

        return false;
    }
}