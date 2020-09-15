using System.Linq;
using Assets.Creature;
using Assets.Structures;

public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;

    public override string Message
    {
        get
        {
            return $"Remove {StructureToRemove.Name} at {StructureToRemove.Cell}";
        }
    }

    public RemoveStructure()
    {
        RequiredSkill = SkillConstants.Build;
        RequiredSkillLevel = 1;
    }

    public override void FinalizeTask()
    {
    }

    public RemoveStructure(Structure structure) : this()
    {
        StructureToRemove = structure;

    }

    public bool Decontructed;

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            if (!creature.Cell.Neighbors.Contains(StructureToRemove.Cell))
            {
                var pathable = Map.Instance.TryGetPathableNeighbour(StructureToRemove.Cell);

                if (pathable != null)
                {
                    AddSubTask(new Move(pathable));
                }
                else
                {
                    creature.SuspendTask(true);
                    return false;
                }
            }

            if (!Decontructed)
            {
                creature.Face(StructureToRemove.Cell);
                AddSubTask(new Wait(StructureToRemove.Cost.Items.Sum(c => c.Value) + 1, "Deconstructing...", AnimationType.Interact));
                Decontructed = true;
                return false;
            }

            foreach (var item in StructureToRemove.Cost.Items)
            {
                var spawnedItem = Game.Instance.ItemController.SpawnItem(item.Key, StructureToRemove.Cell);
                spawnedItem.Amount = item.Value;
                spawnedItem.FactionName = creature.FactionName;
            }

            Game.Instance.StructureController.DestroyStructure(StructureToRemove);
            StructureToRemove = null;
            return true;
        }

        return false;
    }
}