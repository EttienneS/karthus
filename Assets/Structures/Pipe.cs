using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pipe : Structure
{
    public static Direction[] PipeNeighours = new Direction[]
    {
        Direction.N, Direction.E, Direction.S, Direction.W
    };

    public List<Pipe> LinkedPipes
    {
        get
        {
            var linked = new List<Pipe>();
            foreach (var dir in PipeNeighours)
            {
                var n = Cell.GetNeighbor(dir);

                if (n?.Structure?.IsBluePrint == false && n?.Structure?.IsPipe() == true)
                {
                    linked.Add(n.Structure as Pipe);
                }
            }

            return linked;
        }
    }

    public ManaColor? Attunement;

    internal void Flow()
    {
        if (!Attunement.HasValue)
        {
            return;
        }

        var color = Attunement.Value;
        int count = int.MaxValue;
        Pipe target = null;

        var pipes = LinkedPipes.ToList();
        pipes.Shuffle();

        foreach (var linkedpipe in pipes)
        {
            if (!linkedpipe.Attunement.HasValue || !linkedpipe.ManaPool.ContainsKey(color))
            {
                count = 0;
                target = linkedpipe;
                break;
            }
            else
            {
                var amount = linkedpipe.ManaPool[color].Total;
                if (amount < count)
                {
                    count = amount;
                    target = linkedpipe;
                }
            }
        }

        var pressure = ManaPool[color].Total;
        if (target != null && pressure > count)
        {
            var amount = Mathf.Max(1, (pressure - count) / 2);

            ManaPool.Transfer(target.ManaPool, color, amount);

            target.Attunement = color;
            target.Cell.UpdateTile();

            if (ManaPool.HasMana(Attunement.Value))
            {
                Attunement = null;
            }
            Cell.UpdateTile();
        }
    }
}