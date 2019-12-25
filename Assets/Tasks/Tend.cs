using System.Linq;
using UnityEngine;

public class Tend : CreatureTask
{
    public string StructureId;
    public float TendTime;

    public Tend()
    {
        RequiredSkill = SkillConstants.Farming;
        RequiredSkillLevel = 1;
    }

    public Tend(Structure structure, float tendTime) : this()
    {
        StructureId = structure.Id;
        TendTime = tendTime;
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var structure = StructureId.GetStructure();

            if (!creature.Cell.NonNullNeighbors.Contains(structure.Cell))
            {
                AddSubTask(new Move(structure.Cell.GetPathableNeighbour()));
                return false;
            }

            TendTime -= Time.deltaTime;
            creature.Face(structure.Cell);
            if (TendTime <= 0)
            {
                structure.SetValue("Tend", creature.GetSkill("Farming").Level);
                return true;
            }
        }
        return false;
    }
}