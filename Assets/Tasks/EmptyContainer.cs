using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmptyContainer : CreatureTask
{
    public string ContainerId;

    public EmptyContainer()
    {
    }

    public EmptyContainer(Structure container) : this()
    {
        ContainerId = container.Id;
    }


    public override bool Done(Creature Creature)
    {
        return SubTasksComplete(Creature);
    }
}