using Structures;
using Structures.Work;
using System.Linq;

public class DoWork : CreatureTask
{
    public WorkOrderBase Order;
    public string StructureId;

    private Structure _structure;

    public DoWork()
    {
    }

    public DoWork(Structure structure, WorkOrderBase order)
    {
        _structure = structure;
        StructureId = structure.Id;

        RequiredSkill = order.Skill;
        RequiredSkillLevel = order.Option.RequiredSkillLevel;
    }

    public Structure Structure
    {
        get
        {
            if (_structure == null)
            {
                _structure = StructureId.GetStructure();
            }
            return _structure;
        }
    }

    public override void Complete()
    {
        Structure.Free();
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (!creature.Cell.NonNullNeighbors.Contains(Structure.Cell))
            {
                AddSubTask(new Move(Structure.Cell.GetPathableNeighbour()));
            }
            else
            {
                Structure.Reserve(creature);

                creature.SetAnimation(LPC.Spritesheet.Generator.Interfaces.Animation.Thrust, Game.TimeManager.CreatureTick);
                creature.Face(Structure.Cell);
                Order.Progress += Game.TimeManager.CreatureTick;

                if (Order.Progress >= Order.Option.TimeToComplete)
                {
                    Order.UnitComplete(creature.GetSkillLevel(Order.Skill));
                    creature.GainSkill(Order.Skill, 0.1f);
                    Order.Amount--;
                    Order.Progress = 0;
                }

                if (Order.Amount <= 0)
                {
                    Order.OrderComplete();
                    return true;
                }
            }
        }
        return false;
    }
}