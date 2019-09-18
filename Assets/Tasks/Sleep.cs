using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class Sleep : EntityTask
{
    public float RecoveryRate;
    public Cell Location;

    [JsonIgnore]
    private Structure _bed;

    public Sleep()
    {
    }

    public Sleep(Structure bed) : this(bed.Cell, float.Parse(bed.Properties["RecoveryRate"]))
    {
        _bed = bed;
    }

    public Sleep(Cell location, float recoveryRate)
    {
        BusyEmote = Message = "Zzzz.";
        RecoveryRate = recoveryRate;
        Location = location;

        AddSubTask(new Move(location));
    }

    public override bool Done()
    {
        if (_bed != null)
        {
            _bed.Reserve(AssignedEntity.Id);
        }

        if (SubTasksComplete())
        {
            if (CreatureData.ValueProperties[Prop.Energy] < Random.Range(80, 100))
            {
                var wait = new Wait(0.5f, "Sleeping") { AssignedEntity = AssignedEntity };
                AddSubTask(wait);

                CreatureData.ValueProperties[Prop.Energy] += RecoveryRate;
                return false;
            }

            if (RecoveryRate < 1f)
            {
                CreatureData.CreatureRenderer.ShowText("*stretch* Ow my back!", 1f);
            }

            return true;
        }

        return false;
    }
}