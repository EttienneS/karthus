public interface IAttack
{
    int Range { get; set; }
    IEntity Attacker { get; set; }
    IEntity Target { get; set; }

    bool Ready();

    void Resolve();
}