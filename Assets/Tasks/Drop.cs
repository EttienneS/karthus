public class Drop : CreatureTask
{
    internal int Amount;
    internal string ItemId;

    public Drop()
    {

    }

    public Drop(Cell target, Item item, int amount = -1)
    {
        ItemId = item.Id;
        Amount = amount;
        AddSubTask(new Move(target));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var item = ItemId.GetItem();
            creature.DropItem(creature.Cell, item.Name, Amount < 0 ? item.Amount : Amount);
            return true;
        }
        return false;
    }
}