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
    internal Dictionary<int, KeyValuePair<Vector3, Vector3>> LineMoves = new Dictionary<int, KeyValuePair<Vector3, Vector3>>();
    internal float NextUpdate;

    public void JitterLine()
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            var newCell = Cells[i];

            if ((i == 0) || (i == Cells.Count - 1))
            {
                // start or end of line so skip
                continue;
            }

            if (newCell.Structure?.StructureType == "Anchor")
            {
                continue;
            }

            if (Random.value < Jitter)
            {
                var neighbors = newCell.Neighbors.Where(n => n != null
                && n.Neighbors.Contains(Cells[i + 1])
                && n.Neighbors.Contains(Cells[i - 1])).ToList();

                if (neighbors.Any())
                {
                    newCell = neighbors[(int)Random.value * neighbors.Count];
                    LineMoves.Add(i, new KeyValuePair<Vector3, Vector3>(Cells[i].Coordinates.ToTopOfMapVector(),
                                                                newCell.Coordinates.ToTopOfMapVector()));
                    Cells[i] = newCell;
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

            if (LineMoves.Any())
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

    private void Awake()
    {
        Line = GetComponent<LineRenderer>();
        NextUpdate = Random.value * 5;
        DeltaTime = 0;
    }
}