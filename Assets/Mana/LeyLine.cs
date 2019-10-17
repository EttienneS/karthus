using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LeyLine : MonoBehaviour
{
    public List<Cell> Cells = new List<Cell>();
    public LineRenderer Line;

    public ManaColor ManaColor;
    internal float Jitter;
    internal Dictionary<int, KeyValuePair<Vector3, Vector3>> LineMoves = new Dictionary<int, KeyValuePair<Vector3, Vector3>>();
    internal float NextUpdate;

    public void JitterLine()
    {
        Cells = Cells.Distinct().ToList();
        for (int i = 0; i < Cells.Count / 10; i++)
        {
            if (Random.value < Jitter)
                continue;

            var newCell = Cells[Random.Range(0, Cells.Count - 1)];

            if (newCell.Structure?.IsType("Anchor") == true)
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
                LineMoves.Add(index, new KeyValuePair<Vector3, Vector3>(newCell.ToTopOfMapVector(),
                                                            newCell.ToTopOfMapVector()));
                Cells[index] = newCell;
            }
        }
    }

    public void Start()
    {
        Line.positionCount = Cells.Count;
        Line.SetPositions(Cells.Select(c => c.ToTopOfMapVector()).ToArray());
    }

    internal void Awake()
    {
        Line = GetComponent<LineRenderer>();
        NextUpdate = Random.value * 5;
    }
}