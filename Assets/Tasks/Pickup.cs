public class Pickup : CreatureTask
{
    public int Amount;
    public string ItemId;

    public Pickup()
    {
    }

    public Pickup(Item item, int amount = -1)
    {
        ItemId = item.Id;
        Amount = amount;
        AddSubTask(new Move(item.Cell));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (creature.HasItem(ItemId))
            {
                return true;
            }

            var item = ItemId.GetItem();
            if (item == null)
            {
                throw new TaskFailedException("Cannot pick up, item does not exist!");
            }
            creature.PickUpItem(item, Amount < 0 ? item.Amount : Amount);
            return true;
        }
        return false;
    }
}