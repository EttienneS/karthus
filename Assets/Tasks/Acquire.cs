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
                    Item closest = null;
                    foreach (var item in IdService.ItemLookup.Where(i => i.Value.Name.Equals(ItemType, StringComparison.OrdinalIgnoreCase) && !i.Value.InUseByAnyone))
                    {
                        var dist = Pathfinder.Distance(creature.Cell, item.Value.Cell, creature.Mobility);

                        if (dist < distance)
                        {
                            distance = dist;
                            closest = item.Value;
                            if (dist < 10)
                            {
                                // close enough stop searching
                                break;
                            }
                        }
                    }

                    if (closest == null)
                    {
                        throw new TaskFailedException($"No items of required type ({ItemType}) can be found");
                    }
                    else
                    {
                        TargetId = closest.Id;
                        AddSubTask(new Move(closest.Cell));
                    }
                }
                else
                {
                    var target = TargetId.GetItem();

                    if (target.Cell == creature.Cell)
                    {
                        creature.PickUpItem(target, Amount - creature.CurrentItemCount(ItemType));
                        TargetId = null;
                    }
                }
            }
        }

        return false;
    }
}