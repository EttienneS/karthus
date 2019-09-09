using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeyLineController : MonoBehaviour
{
    public float Jitter = 0.1f;
    public LeyLine LeyLinePrefab;
    public ChannelLine ChannelLinePrefab;

    public List<LeyLine> Lines = new List<LeyLine>();

    public LeyLine MakeLine(List<CellData> cells, ManaColor manaColor)
    {
        var line = Instantiate(LeyLinePrefab, transform);

        line.Jitter = Jitter;
        line.Line.material = Game.MaterialController.GetLeyLineMaterial(manaColor.GetActualColor());
        line.ManaColor = manaColor;
        line.name = $"{cells.First()}-{cells.Last()}";
        Lines.Add(line);

        line.Cells.AddRange(cells);

        
        line.JitterLine();
        return line;
    }

    public ChannelLine MakeChannellingLine(IEntity source, IEntity target, int intensity, float duration, ManaColor manaColor)
    {
        var line = Instantiate(ChannelLinePrefab, transform);
        line.name = $"Channel: {manaColor}";
        line.SetProperties(source, target, intensity, duration, manaColor);

        return line;
    }
}