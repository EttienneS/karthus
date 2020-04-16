public class Drop : CreatureTask
{
    public float TargetX;
    public float TargetY;
    public override string Message
    {
        get
        {
            return $"Drop held item at {TargetX}:{TargetY}";
        }
    }

    public Drop()
    {
    }

    public Drop(Cell target)
    {
        TargetX = target.X;
        TargetY = target.Y;
        AddSubTask(new Move(target));
    }

    public override void Complete()
    {
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.DropItem(creature.Cell);
            return true;
        }
        return false;
    }
}