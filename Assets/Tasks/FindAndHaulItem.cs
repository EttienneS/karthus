using Newtonsoft.Json;
using Assets.Creature;

public class FindAndHaulItem : CreatureTask
{
    internal int Amount;

    internal string ItemType;

    internal float TargetX;

    internal float TargetZ;

    internal string DestinationEntityId;


    public override string Message
    {
        get
        {
            return $"Find and move {Amount} of {ItemType} to {TargetX}:{TargetZ}";
        }
    }

    public override void Complete()
    {
    }

    public FindAndHaulItem()
    {
        RequiredSkill = SkillConstants.Haul;
        RequiredSkillLevel = 1;
    }

    public FindAndHaulItem(string itemType, int amount, Cell target, IEntity destinationEntity) : this()
    {
        TargetX = target.Vector.x;
        TargetZ = target.Vector.z;
        if (destinationEntity != null)
        {
            DestinationEntityId = destinationEntity.Id;
        }
        ItemType = itemType;
        Amount = amount;
        AddSubTask(new FindAndGetItem(itemType, amount));
        AddSubTask(new Move(target));
    }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Instance.Map.GetCellAtCoordinate(TargetX, TargetZ);
        }
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            var item = creature.DropItem(TargetCell);

            if (!string.IsNullOrEmpty(DestinationEntityId))
            {
                item.Reserve(DestinationEntityId.GetEntity());
            }
            return true;
        }
        return false;
    }
}