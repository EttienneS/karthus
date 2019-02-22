using System.Collections.Generic;

public class CellPriorityQueue
{
    private readonly List<CellData> list = new List<CellData>();
    private int minimum = int.MaxValue;

    public CellPriorityQueue()
    {
        Count = 0;
    }

    public int Count { get; private set; }

    public void Enqueue(CellData cell)
    {
        Count++;
        var priority = cell.SearchPriority;

        if (priority < minimum)
        {
            minimum = priority;
        }

        while (priority >= list.Count)
        {
            list.Add(null);
        }

        cell.NextWithSamePriority = list[priority];

        list[priority] = cell;
    }

    public CellData Dequeue()
    {
        Count--;

        for (; minimum < list.Count; minimum++)
        {
            var cell = list[minimum];
            if (cell != null)
            {
                list[minimum] = cell.NextWithSamePriority;
                return cell;
            }
        }

        return null;
    }

    public void Change(CellData cell, int oldPriority)
    {
        var current = list[oldPriority];
        var next = current.NextWithSamePriority;

        if (current == cell)
        {
            list[oldPriority] = next;
        }
        else
        {
            while (next != cell)
            {
                current = next;
                next = current.NextWithSamePriority;
            }

            current.NextWithSamePriority = cell.NextWithSamePriority;
        }

        Enqueue(cell);
        Count--;
    }

    public void Clear()
    {
        list.Clear();
        Count = 0;
        minimum = int.MaxValue;
    }
}