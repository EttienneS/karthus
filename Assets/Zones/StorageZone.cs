using System.Collections.Generic;
using System.Linq;

public class StorageZone : ZoneBase
{
    public Dictionary<string, int> StorageDefinition = new Dictionary<string, int>();

    public int Capacity
    {
        get
        {
            var total = 0;
            foreach (var container in Containers)
            {
                total += container.RemainingCapacity();
            }
            return total;
        }
    }

    public IEnumerable<Structure> Containers
    {
        get
        {
            return Structures.Where(s => s.IsContainer());
        }
    }

    public bool CanStore(string name, string category, int amount)
    {
        return true;
    }
}