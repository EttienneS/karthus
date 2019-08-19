using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LeyLine : MonoBehaviour
{
    public List<CellData> Cells = new List<CellData>();
    public LineRenderer Line;

    public ManaColor ManaColor;
    internal float DeltaTime;
    internal bool FirstDraw = true;
    internal float Jitter;
    internal int MaxChanges = 3;
    internal Dictionary<int, KeyValuePair<Vector3, Vector3>> LineMoves = new Dictionary<int, KeyValuePair<Vector3, Vector3>>();
    internal float NextUpdate;

    public void JitterLine()
    {
        if (FirstDraw)
        {
            JitterDiagonals();
        }

        for (int i = 0; i < MaxChanges; i++)
        {
            if (Random.value < Jitter)
                continue;

            var newCell = Cells[Random.Range(0, Cells.Count - 1)];

            if (newCell.Structure?.StructureType == "Anchor")
            {
                continue;
            }

            var index = Cells.IndexOf(newCell);
            if (LineMoves.ContainsKey(index))
            {
                // line is currently moving, skip
                continue;
            }

            var previous = Cells[index - 1];
            var next = Cells[index + 1];

            var neighbors = newCell.Neighbors.Where(n => n != null
                                && !Cells.Contains(n)
                                && n.Neighbors.Contains(previous)
                                && n.Neighbors.Contains(next)).ToList();

            if (neighbors.Count > 0)
            {
                newCell = neighbors[(int)Random.value * neighbors.Count];
                LineMoves.Add(index, new KeyValuePair<Vector3, Vector3>(newCell.Coordinates.ToTopOfMapVector(),
                                                            newCell.Coordinates.ToTopOfMapVector()));
                Cells[index] = newCell;
            }
        }
    }

    public void JitterDiagonals()
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            var cell = Cells[i];
            if (i == 0 || i == Cells.Count - 1 || cell.Structure?.StructureType == "Anchor")
            {
                continue;
            }

            var previous = Cells[i - 1];
            var next = Cells[i + 1];

            if (cell.Coordinates.X != next.Coordinates.X
                && cell.Coordinates.X != previous.Coordinates.X
                && cell.Coordinates.Y != next.Coordinates.Y
                && cell.Coordinates.Y != previous.Coordinates.Y)
            {
                if (Random.value < 0.5f)
                {
                    var neighbors = cell.Neighbors.Where(n => n != null
                                        && n != next 
                                        && n != previous).ToList();

                    if (neighbors.Count > 0)
                    {
                        var insert = neighbors[(int)Random.value * neighbors.Count];
                        Cells.Insert(i, insert);
                        i++;
                    }

                }
            }
        }
    }

    public void Update()
    {
        if (FirstDraw)
        {
            Line.positionCount = Cells.Count;
            Line.SetPositions(Cells.Select(c => c.Coordinates.ToTopOfMapVector()).ToArray());

            FirstDraw = false;
        }
        else
        {
            DeltaTime += Time.deltaTime;

            if (LineMoves.Count > 0)
            {
                foreach (var kvp in LineMoves)
                {
                    var index = kvp.Key;
                    var from = kvp.Value.Key;
                    var to = kvp.Value.Value;

                    Line.SetPosition(index, Vector3.Lerp(from, to, DeltaTime));
                }

                if (DeltaTime >= 1)
                {
                    LineMoves.Clear();
                    DeltaTime = 0;
                }
            }
            else
            {
                if (DeltaTime > NextUpdate)
                {
                    NextUpdate = (Random.value * 5) + 5;
                    DeltaTime = 0;
                    JitterLine();
                }
            }
        }
    }

    internal void Awake()
    {
        Line = GetComponent<LineRenderer>();
        NextUpdate = Random.value * 5;
        DeltaTime = 0;
    }
}