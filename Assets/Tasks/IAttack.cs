using System;

public interface IAttack
{
    bool Resolve(IEntity target);
}

public class Blast : IAttack
{
    public bool Resolve(IEntity target)
    {
        throw new NotImplementedException();
    }
}