using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class StorageZone : ZoneBase
{
    public Dictionary<string, int> StorageDefinition = new Dictionary<string, int>();

    public string Filter = "*";

    [JsonIgnore]
    public int Fill
    {
        get
        {
            var total = 0;
            foreach (var container in Containers)
            {
                total += container.RemainingCapacity;
            }
            return total;
        }
    }

    [JsonIgnore]
    public int Capacity
    {
        get
        {
            var total = 0;
            foreach (var container in Containers)
            {
                total += container.Capacity;
            }
            return total;
        }
    }

    [JsonIgnore]
    public IEnumerable<Container> Containers
    {
        get
        {
            return Structures.OfType<Container>();
        }
    }

    public bool CanStore(string name, string category, int amount)
    {
        return true;
    }

    public void SetFilter(string filter)
    {
        Filter = filter;
        foreach (var container in Containers)
        {
            container.Filter = filter;
        }
    }
}