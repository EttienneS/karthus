using Assets.Creature;
using Structures;
using Structures.Work;
using System.Linq;

public class DoWork : CreatureTask
{
    public WorkOrderBase Order;
    public string StructureId;

    private Structure _structure;

    public override string Message
    {
        get
        {
            return $"{Order.Name} at {_structure.Name}";
        }
    }

    public DoWork()
    {
    }

    public DoWork(Structure structure, WorkOrderBase order)
    {
        _structure = structure;
        StructureId = structure.Id;
        Order = order;
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

    public override bool Done(CreatureData creature)
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

                creature.SetAnimation(AnimationType.Interact);
                creature.Face(Structure.Cell);
                Order.Progress += Game.Instance.TimeManager.CreatureTick;

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
                    creature.SetAnimation(AnimationType.Idle);
                    return true;
                }
            }
        }
        return false;
    }
}