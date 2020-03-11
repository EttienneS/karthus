using Needs;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;
using Structures;
public class Sleep : CreatureTask
{
    public string BedId;
    public float RecoveryRate = 1.25f;

    public Structure Bed
    {
        get
        {
            return BedId.GetStructure();
        }
    }

    public Sleep()
    {
        BusyEmote = "Zzzz...";
    }

    public Sleep(string bedId) : this()
    {
        if (string.IsNullOrEmpty(bedId))
        {
            BedId = bedId;
            AddSubTask(new Move(Bed.Cell));
        }
    }


    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.SetFixedAnimation(Animation.Walk, 1);
            creature.GetNeed<Energy>().CurrentChangeRate = RecoveryRate;

            if (creature.GetCurrentNeed<Energy>() > 90f)
            {
                creature.GetNeed<Energy>().ResetRate();
                return true;
            }
        }

        return false;
    }
}