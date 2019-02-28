using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class Sleep : TaskBase
{
    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.Energy < Random.Range(80,100))
            {
                var wait = new Wait(0.5f, "Sleeping") { CreatureId = CreatureId };
                SubTasks.Enqueue(wait);

                Creature.LinkedGameObject.ShowText("Zzz..", 0.25f);
                return false;
            }

            Creature.LinkedGameObject.ShowText("*stretch* Ow my back!", 1f);
            Creature.Sleeping = false;
            return true;
        }

        return false;
    }

    public override void Update()
    {
        Creature.Sleeping = true;
        Taskmaster.ProcessQueue(SubTasks);
    }
}