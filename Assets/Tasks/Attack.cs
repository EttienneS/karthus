
public class ExecuteAttack : TaskBase
{
    public ExecuteAttack()
    {
    }

    public IEntity Target;
    public IAttack Attack;

    public ExecuteAttack(IEntity target, IAttack attack)
    {
        Target = target;
        Attack = attack;
    }

    public override bool Done()
    {
        Attack.Attacker = AssignedEntity;

        if (Faction.QueueComplete(SubTasks) && Attack.Ready())
        {
            return Attack.Resolve(Target);
        }

        return false;
    }
}