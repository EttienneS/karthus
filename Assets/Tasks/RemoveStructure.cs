using System.Collections.Generic;
using System.Linq;

public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;
    public RemoveStructure()
    {
        RequiredSkill = "Build";
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
                Game.ItemController.SpawnItem(item.Key, StructureToRemove.Cell).Amount = item.Value;
            }

            Game.StructureController.DestroyStructure(StructureToRemove);

            return true;
        }

        return false;
    }
}