using System.Linq;

public class Bite : IAttack
{
    public int Range { get; set; } = 0;
    public IEntity Attacker { get; set; }
    public IEntity Target { get; set; }

    public bool Ready()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }
        if (Attacker.Cell.DistanceTo(Target.Cell) > Range)
        {
            Attacker.Task.AddSubTask(new Move(Target.Cell));
            return false;
        }
        return true;
    }

    public void Resolve()
    {
        if (Target == null)
        {
            throw new TaskFailedException();
        }
        Attacker.Task.DoneEmote = "OMNOMONOM";
        Target.Damage(2, ManaColor.Black);
        Attacker.Task.AddSubTask(new Move(Target.Cell.Neighbors.Where(n => n != null && n.TravelCost > 0).ToList().GetRandomItem()));
    }
}