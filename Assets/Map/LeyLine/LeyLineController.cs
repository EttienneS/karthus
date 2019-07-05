using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeyLineController : MonoBehaviour
{
    public float Jitter = 0.1f;
    public LeyLine LeyLinePrefab;

    public List<LeyLine> Lines = new List<LeyLine>();

    public LeyLine MakeLine(List<CellData> cells, ManaColor manaColor)
    {
        var line = Instantiate(LeyLinePrefab, transform);

        line.Jitter = Jitter;
        line.Line.material.SetColor("_EffectColor", manaColor.GetActualColor());
        line.ManaColor = manaColor;
        line.name = $"{cells.First().Coordinates}-{cells.Last().Coordinates}";
        Lines.Add(line);

        line.Cells.AddRange(cells);
        
        line.JitterLine();
        return line;
    }
}