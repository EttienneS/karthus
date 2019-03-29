﻿using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class Sleep : TaskBase
{
    public float RecoveryRate;
    public Coordinates Location;

    [JsonIgnore]
    private StructureData _bed;

    public Sleep()
    {
    }

    public Sleep(StructureData bed) : this(bed.Coordinates, float.Parse(bed.Properties["RecoveryRate"]))
    {
        _bed = bed;
    }

    public Sleep(Coordinates location, float recoveryRate)
    {
        BusyEmote = Message = "Zzzz..";
        RecoveryRate = recoveryRate;
        Location = location;

        AddSubTask(new Move(location));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.Energy < Random.Range(80, 100))
            {
                var wait = new Wait(0.5f, "Sleeping") { AssignedCreatureId = AssignedCreatureId };
                AddSubTask(wait);

                Creature.Energy += RecoveryRate;
                return false;
            }

            if (RecoveryRate < 1f)
            {
                Creature.LinkedGameObject.ShowText("*stretch* Ow my back!", 1f);
            }

            Creature.Animate = true;
            return true;
        }

        return false;
    }

    public override void Update()
    {
        if (_bed != null)
        {
            _bed.Reserve(Creature.GetGameId());
            Creature.Animate = false;
        }
        Taskmaster.ProcessQueue(SubTasks);
    }
}