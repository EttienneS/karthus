﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public enum MemoryType
{
    Item, Location, Creature, Structure
}

public class Memory : Dictionary<MemoryType, List<string>>
{
    public string AddInfo(MemoryType type, string entry)
    {
        if (!ContainsKey(type))
        {
            Add(type, new List<string>());
        }

        this[type].Add(entry);

        return entry;
    }

    [JsonIgnore]
    public IEnumerable<Structure> Structures
    {
        get
        {
            if (ContainsKey(MemoryType.Structure))
            {
                return this[MemoryType.Structure].Select(IdExtensions.GetStructure);
            }
            return new List<Structure>();
        }
    }
}