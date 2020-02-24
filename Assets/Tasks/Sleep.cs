using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

public class Sleep : CreatureTask
{
    public string BedId;
    public float RecoveryRate = 2f;

    public Structure Bed
    {
        get
        {
            return BedId.GetStructure();
        }
    }

    public Sleep()
    {
    }

    public Sleep(string bedId) : this()
    {
        if (string.IsNullOrEmpty(bedId))
        {
            BedId = bedId;
            RecoveryRate = Bed.GetValue("RecoveryRate");
            AddSubTask(new Move(Bed.Cell));
        }
    }

    public float Delta = 0;

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.SetFixedAnimation(Animation.Walk, 1);

            Delta += Time.deltaTime;

            if (Delta > 1f)
            {
                Delta--;
                creature.DecreaseNeed(NeedNames.Energy, RecoveryRate);
            }

            if (creature.GetCurrentNeed(NeedNames.Energy) < 10f && Random.value > 0.75f)
            {
                return true;
            }
        }

        return false;
    }
}