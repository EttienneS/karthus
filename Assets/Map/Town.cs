using System.Collections.Generic;
using System.Linq;

public class Town
{
    public List<CellData> Cells;
    public CellData Center;
    public int Radius;

    public Town(CellData center, List<CellData> demense, int radius)
    {
        Center = center;
        Cells = demense;
        Radius = radius;
    }

    public float Height
    {
        get
        {
            return Cells.Where(c => c.TravelCost > 0).Average(c => c.Height);
        }
    }

    public void AddCell(CellData cell)
    {
        Cells.Add(cell);
    }
}