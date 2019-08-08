
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
        return Attack.Resolve(Target);
    }
}