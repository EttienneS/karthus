using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Assets.Structures;

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
          
            return total;
        }
    }

    [JsonIgnore]
    public int Capacity
    {
        get
        {
            var total = 0;
        
            return total;
        }
    }

    public bool CanStore(string name, string category, int amount)
    {
        return true;
    }
    
    public void SetFilter(string filter)
    {
        Filter = filter;
    }
}