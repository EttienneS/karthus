using Assets.Creature;
using Structures.Work;

public class DoWork : CreatureTask
{
    public WorkOrderBase Order;
    public string StructureId;

    private WorkStructureBase _structure;

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

    public DoWork(WorkStructureBase structure, WorkOrderBase order)
    {
        _structure = structure;
        StructureId = structure.Id;
        Order = order;
        RequiredSkill = order.Skill;
        RequiredSkillLevel = order.Option.RequiredSkillLevel;
    }

    public WorkStructureBase WorkStructure
    {
        get
        {
            if (_structure == null)
            {
                _structure = StructureId.GetStructure() as WorkStructureBase;
            }
            return _structure;
        }
    }

    public override void FinalizeTask()
    {
        WorkStructure.Free();
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            var workCell = WorkStructure.GetWorkCell();
            if (creature.Cell != workCell)
            {
                AddSubTask(new Move(workCell));
            }
            else
            {
                WorkStructure.Reserve(creature);

                if (Order.HasMaterial())
                {
                    creature.SetAnimation(AnimationType.Interact);
                    creature.Face(WorkStructure.Cell);
                    Order.Progress += 0.5f;

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
                else
                {
                    foreach (var material in Order.GetRequiredMaterial())
                    {
                        AddSubTask(new FindAndHaulItem(material.Key, material.Value, WorkStructure.Cell));
                    }
                }
            }
        }
        return false;
    }
}