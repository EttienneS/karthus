using Newtonsoft.Json;
using Assets.Creature;

public class FindAndHaulItem : CreatureTask
{
    internal int Amount;

    internal string ItemType;

    internal float TargetX;

    internal float TargetZ;

    public override string Message
    {
        get
        {
            return $"Find and move {Amount} of {ItemType} to {TargetX}:{TargetZ}";
        }
    }

    public override void FinalizeTask()
    {
    }

    public FindAndHaulItem()
    {
        RequiredSkill = SkillConstants.Haul;
        RequiredSkillLevel = 1;
    }

    public FindAndHaulItem(string itemType, int amount, Cell target) : this()
    {
        TargetX = target.Vector.x;
        TargetZ = target.Vector.z;
       
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
            return Map.Instance.GetCellAtCoordinate(TargetX, TargetZ);
        }
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.DropItem(TargetCell);

            return true;
        }
        return false;
    }
}