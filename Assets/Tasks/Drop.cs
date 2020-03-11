public class Drop : CreatureTask
{
    public Drop()
    {
    }

    public Drop(Cell target)
    {
        AddSubTask(new Move(target));
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