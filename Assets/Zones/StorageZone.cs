using Assets.Item;
using System.Collections.Generic;
using System.Linq;

public class StorageZone : ZoneBase
{
    public StorageFilter Filter;

    private readonly List<Cell> _reservedCells = new List<Cell>();

    public bool CanStore(ItemData item)
    {
        return Filter.Allows(item);
    }

    public Cell GetReservedCellFor(ItemData item)
    {
        if (!CanStore(item))
        {
            throw new ItemNotAllowedInStoreException(item);
        }

        foreach (var cell in GetOpenCells().Where(c => !c.Items.Any()))
        {
            _reservedCells.Add(cell);
            return cell;
        }

        throw new NoCellFoundException($"No cell found to store {item}");
    }

    public int GetFreeCellCount()
    {
        return GetItemCapacity() - GetOpenCells().Count();
    }

    public int GetItemCapacity()
    {
        return GetCells().Count;
    }

    public IEnumerable<Cell> GetOpenCells()
    {
        FreeFilledReservedCells();
        return GetCells().Except(_reservedCells);
    }

    private void FreeFilledReservedCells()
    {
        _reservedCells.RemoveAll(c => c.Items.Any());
    }
}