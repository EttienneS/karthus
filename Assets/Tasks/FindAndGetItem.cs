public class FindAndGetItem : CreatureTask
{
    internal string ItemCriteria;
    internal int Amount;
    internal string TargetId;

    public FindAndGetItem(string itemType, int amount)
    {
        ItemCriteria = itemType;
        Amount = amount;
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (creature.HasItem(ItemCriteria, Amount))
            {
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(TargetId))
                {
                    IEntity targetEntity = creature.Faction.FindItemOrContainer(ItemCriteria, creature);

                    if (targetEntity == null)
                    {
                        throw new TaskFailedException($"No items of required type ({ItemCriteria}) can be found");
                    }
                    else
                    {
                        AddSubTask(new Move(targetEntity.Cell));
                        TargetId = targetEntity.Id;
                    }
                }
                else
                {
                    var targetItem = TargetId.GetItem();
                    var requiredAmount = Amount - creature.CurrentItemCount(ItemCriteria);
                    if (targetItem != null)
                    {
                        creature.PickUpItem(targetItem, requiredAmount);
                    }
                    else
                    {
                        var container = TargetId.GetContainer();
                        var item = container.GetItem(requiredAmount);
                        if (item != null)
                        {
                            creature.PickUpItem(item, requiredAmount);
                        }
                        container.Free();
                    }

                    TargetId = null;
                }
            }
        }

        return false;
    }
}