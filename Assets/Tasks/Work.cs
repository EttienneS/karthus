using System.Linq;
using UnityEngine;

public class Work : CreatureTask
{
    public string StructureId;
    public float WorkTime;

    public Work(Structure structure, float workTime)
    {
        StructureId = structure.Id;
        WorkTime = workTime;
    }

    public override bool Done(Creature creature)
    {
        var structure = StructureId.GetStructure();

        if (!structure.InUseByAnyone)
        {
            structure.Reserve(creature);
        }
        else if (structure.InUseById == creature.Id)
        {
            // structure busy
            return false;
        }

        if (SubTasksComplete(creature))
        {
            if (!creature.Cell.NonNullNeighbors.Contains(structure.Cell))
            {
                AddSubTask(new Move(structure.Cell.GetPathableNeighbour()));
                return false;
            }

            WorkTime -= Time.deltaTime;
            creature.Face(structure.Cell);
            if (WorkTime <= 0)
            {
                structure.SetValue("Work", structure.GetValue("Work") + creature.GetSkill(RequiredSkill).RollSkill());
                creature.GainSkill(RequiredSkill, RequiredSkillLevel / 10);
                structure.Free();
                return true;
            }
        }
        return false;
    }
}