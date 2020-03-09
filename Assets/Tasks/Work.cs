using System.Linq;
using UnityEngine;
using Structures;

public class Work : CreatureTask
{
    public string StructureId;
    public float WorkTime;
    public string BusySprite;

    public Work()
    {
    }

    public Work(Structure structure, float workTime)
    {
        StructureId = structure.Id;
        WorkTime = workTime;
    }

    public VisualEffect _creatureEffect;

    public override bool Done(Creature creature)
    {
        var structure = StructureId.GetStructure();

        if (SubTasksComplete(creature))
        {
            if (!creature.Cell.NonNullNeighbors.Contains(structure.Cell))
            {
                AddSubTask(new Move(structure.Cell.GetPathableNeighbour()));
                return false;
            }

            if (_creatureEffect == null)
            {
                _creatureEffect = Game.VisualEffectController.SpawnSpriteEffect(creature, creature.Vector, string.IsNullOrEmpty(BusySprite) ? "time_t" : BusySprite, WorkTime);
            }
            WorkTime -= Time.deltaTime;
            creature.Face(structure.Cell);
            if (WorkTime <= 0)
            {
                structure.SetValue("Work", structure.GetValue("Work") + creature.GetSkill(RequiredSkill).RollSkill());
                creature.GainSkill(RequiredSkill, RequiredSkillLevel / 10);
                _creatureEffect.DestroySelf();
                return true;
            }
        }
        return false;
    }
}