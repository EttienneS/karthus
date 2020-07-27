using Assets.Item;
using System.Collections.Generic;
using System.Linq;

public class StorageZone : ZoneBase
{
    internal StorageFilter Filter = new StorageFilter();

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
        return GetCells().Count;
    }

    public IEnumerable<Cell> GetOpenCells()
    {
        FreeFilledReservedCells();

        return GetCells()
                    .Where(c => !c.ContainsItems())
                    .Except(_reservedCells);
    }

    private void FreeFilledReservedCells()
    {
        _reservedCells.RemoveAll(c => c.ContainsItems());
    }
}