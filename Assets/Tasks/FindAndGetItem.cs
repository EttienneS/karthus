using UnityEngine;
using Assets.Creature;

public class FindAndGetItem : CreatureTask
{
    internal string ItemCriteria;
    internal int Amount;
    internal string TargetId;

    public override string Message
    {
        get
        {
            return $"Find and pick up {Amount} of {ItemCriteria}";
        }
    }

    public FindAndGetItem(string itemType, int amount)
    {
        ItemCriteria = itemType;
        Amount = amount;
    }

    public override void FinalizeTask()
    {
    }

    public override bool Done(CreatureData creature)
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
                    var item = creature.Faction.FindItem(ItemCriteria, creature);

                    if (item == null)
                    {
                        if (ItemCriteria == "Water")
                        {
                            AddSubTask(new GetWaterFromSource());
                            return false;
                        }
                        throw new TaskFailedException($"No items of required type ({ItemCriteria}) can be found");
                    }
                    else
                    {
                        if (item.Cell.PathableWith(creature.Mobility))
                        {
                            AddSubTask(new Move(item.Cell));
                        }
                        else
                        {
                            AddSubTask(new Move(item.Cell.GetPathableNeighbour()));
                        }
                        TargetId = item.Id;
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
                        TargetId = null;
                    }
                }
            }
        }

        return false;
    }
}