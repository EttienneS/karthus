using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pipe : Structure
{
    public IEnumerable<Pipe> LinkedPipes
    {
        get
        {
            return Cell.Neighbors.Where(n => n?.Structure?.IsPipe() == true && !n.Structure.IsBluePrint)
                                 .Select(c => c.Structure as Pipe);
        }
    }

    public int Pressure
    {
        get
        {
            return (int)ValueProperties[PipeConstants.Pressure];
        }
        set
        {
            ValueProperties[PipeConstants.Pressure] = value;

            if (value < 0)
            {
                Content = null;
            }

            Cell.UpdateTile();
        }
    }

    public ManaColor? Content
    {
        get
        {
            var content = Properties[PipeConstants.Content];

            if (string.IsNullOrWhiteSpace(content) || content == PipeConstants.Nothing)
            {
                return null;
            }

            return (ManaColor)Enum.Parse(typeof(ManaColor), content);
        }
        set
        {
            if (value != null)
            {
                Properties[PipeConstants.Content] = value.ToString();
            }
            else
            {
                Properties[PipeConstants.Content] = PipeConstants.Nothing;
            }
        }
    }

    internal void Flow()
    {
        var linkedPipes = LinkedPipes;

        if (!Content.HasValue || Pressure <= 0)
        {
            Content = null;
            Pressure = 0;
            return;
        }

        foreach (var linkedpipe in linkedPipes)
        {
            var targetContent = linkedpipe.Content;
            var targetPressure = linkedpipe.Pressure;

            var pressure = Pressure;
            if ((targetContent.HasValue || targetContent == null) && targetPressure < pressure)
            {
                linkedpipe.Content = Content;

                var pressureDiff = Mathf.Max(1, (pressure - targetPressure) / 2);
                Pressure -= pressureDiff;
                linkedpipe.Pressure += pressureDiff;
            }
        }
    }

}
