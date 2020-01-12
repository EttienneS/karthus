using System;
using System.Linq;

public class Acquire : CreatureTask
{
    internal string ItemType;
    internal int Amount;
    internal string TargetId;

    public Acquire(string itemType, int amount)
    {
        ItemType = itemType;
        Amount = amount;
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (creature.HasItem(ItemType, Amount))
            {
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(TargetId))
                {
                    var distance = float.MaxValue;
                    Structure container = null;
                    Item closestItem = null;

                    foreach (var structure in creature.Faction.Containers.Where(s => s.ItemType == ItemType))
                    {
                        if (structure.Count > 0)
                        {
                            var dist = Pathfinder.Distance(creature.Cell, structure.Cell, creature.Mobility);
                            if (dist < distance)
                            {
                                distance = dist;
                                container = structure;
                            }
                        }
                    }

                    if (container == null)
                    {
                        foreach (var item in IdService.ItemLookup.Where(i => i.Value.Name.Equals(ItemType, StringComparison.OrdinalIgnoreCase) && !i.Value.InUseByAnyone))
                        {
                            var dist = Pathfinder.Distance(creature.Cell, item.Value.Cell, creature.Mobility);

                            if (dist < distance)
                            {
                                distance = dist;
                                closestItem = item.Value;
                                if (dist < 10)
                                {
                                    // close enough stop searching
                                    break;
                                }
                            }
                        }
                    }

                    if (closestItem == null && container == null)
                    {
                        throw new TaskFailedException($"No items of required type ({ItemType}) can be found");
                    }
                    else
                    {
                        if (container != null)
                        {
                            TargetId = container.Id;
                            AddSubTask(new Move(container.Cell));
                        }
                        else
                        {
                            TargetId = closestItem.Id;
                            AddSubTask(new Move(closestItem.Cell));
                        }
                    }
                }
                else
                {
                    var targetItem = TargetId.GetItem();
                    var requiredAmount = Amount - creature.CurrentItemCount(ItemType);
                    if (targetItem != null)
                    {
                        creature.PickUpItem(targetItem, requiredAmount);
                    }
                    else
                    {
                        var container = TargetId.GetContainer();
                        var item = container.GetItem(ItemType, requiredAmount);
                        if (item != null)
                        {
                            creature.PickUpItem(item, requiredAmount);
                        }
                    }

                    TargetId = null;
                }
            }
        }

        return false;
    }
}