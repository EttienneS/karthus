using Assets;
using Assets.Item;
using System;
using System.Collections.Generic;
using System.Linq;

public class StorageZone : ZoneBase
{
    internal Filter Filter = new Filter();

    private readonly List<Cell> _reservedCells = new List<Cell>();

    public bool CanStore(ItemData item)
    {
        return GetFreeCellCount() > 0 && Filter.Allows(item);
    }

    public Cell GetReservedCellFor(ItemData item)
    {
        if (!CanStore(item))
        {
            throw new ItemNotAllowedInStoreException(item);
        }

        foreach (var cell in GetOpenCells())
        {
            _reservedCells.Add(cell);
            return cell;
        }

        throw new NoCellFoundException($"No cell found to store {item}");
    }

    public int GetFreeCellCount()
    {
        return GetOpenCells().Count();
    }

    public int GetMaxItemCapacity()
    {
        return ZoneCells.GetCells().Count;
    }

    public IEnumerable<Cell> GetOpenCells()
    {
        FreeFilledReservedCells();

        return ZoneCells.GetCells()
                    .Where(CellCanHoldItem)
                    .Except(_reservedCells);
    }

    public bool CellCanHoldItem(Cell cell)
    {
        return cell.PathableWith(Mobility.Walk) && !cell.ContainsItems();
    }

    private void FreeFilledReservedCells()
    {
        _reservedCells.RemoveAll(c => c.ContainsItems());
    }

    internal Cell GetLocation()
    {
        return ZoneCells.GetCells()[0];
    }
}