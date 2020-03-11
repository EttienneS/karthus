using UnityEngine;

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

    public override void Complete()
    {
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (creature.HeldItem?.IsType(ItemCriteria) == true)
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
                        if (targetEntity.Cell.Pathable(creature.Mobility))
                        {
                            AddSubTask(new Move(targetEntity.Cell));
                        }
                        else
                        {
                            AddSubTask(new Move(targetEntity.Cell.GetPathableNeighbour()));
                        }
                        TargetId = targetEntity.Id;
                    }
                }
                else
                {
                    var targetItem = TargetId.GetItem();
                    if (targetItem == null)
                    {
                        TargetId = null;
                        Debug.Log("Item no longer exists");
                    }
                    else
                    {
                        var requiredAmount = Amount;
                        if (creature.HeldItem != null)
                        {
                            requiredAmount -= creature.HeldItem.Amount;
                        }

                        if (creature.HeldItem == null)
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
        }

        return false;
    }
}