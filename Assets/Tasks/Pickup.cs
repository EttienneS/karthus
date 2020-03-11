using System.Linq;

public class Pickup : CreatureTask
{
    public int Amount;
    public string ItemId;

    public Pickup()
    {
    }

    public override void Complete()
    {
    }

    public Pickup(Item item, int amount = -1)
    {
        ItemId = item.Id;
        Amount = amount;
    }

    public override bool Done(Creature creature)
    {
        var item = ItemId.GetItem();
        if (item == null)
        {
            throw new TaskFailedException("Cannot pick up, item does not exist!");
        }
        else
        {
            if (!item.CanUse(creature))
            {
                throw new TaskFailedException($"Cannot pick up, item in use!");
            }
        }

        if (SubTasksComplete(creature))
        {
            if (creature.Cell != item.Cell && !creature.Cell.NonNullNeighbors.Contains(item.Cell))
            {
                AddSubTask(new Move(item.Cell.GetPathableNeighbour()));
                return false;
            }
            if (creature.Cell.NonNullNeighbors.Contains(item.Cell))
            {
                if (item.Cell.Pathable(Mobility.Walk))
                {
                    AddSubTask(new Move(item.Cell));
                }
                else
                {
                    AddSubTask(new Move(item.Cell.GetPathableNeighbour()));
                }
            }

            if (creature.HeldItem == item)
            {
                return true;
            }

            creature.PickUpItem(item, Amount < 0 ? item.Amount : Amount);
            return true;
        }
        return false;
    }
}