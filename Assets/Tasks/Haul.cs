﻿using Newtonsoft.Json;

public class Haul : CreatureTask
{
    internal int Amount;

    internal string ItemType;

    internal float TargetX;

    internal float TargetY;

    internal string DestinationEntityId;

    public Haul()
    {
        RequiredSkill = "Haul";
        RequiredSkillLevel = 1;
    }

    public Haul(string itemType, int amount, Cell target, IEntity destinationEntity) : this()
    {
        TargetX = target.Vector.x;
        TargetY = target.Vector.y;
        if (destinationEntity != null)
        {
            DestinationEntityId = destinationEntity.Id;
        }
        ItemType = itemType;
        Amount = amount;
        AddSubTask(new Acquire(itemType, amount));
        AddSubTask(new Move(target));
    }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Map.GetCellAtCoordinate(TargetX, TargetY);
        }
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var item = creature.DropItem(TargetCell, ItemType, Amount);

            if (!string.IsNullOrEmpty(DestinationEntityId))
            {
                var entity = DestinationEntityId.GetEntity();
                item.InUseBy = entity;

                if (!entity.Properties.ContainsKey(NamedProperties.ContainedItemIds))
                {
                    entity.Properties.Add(NamedProperties.ContainedItemIds, "");
                }

                entity.Properties[NamedProperties.ContainedItemIds] += item.Id + ",";
            }
            return true; 
        }
        return false;
    }
}