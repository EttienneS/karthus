using System.Collections.Generic;

public class Zone
{
    public string Name { get; set; }

    public List<Structure> Structures { get; set; } = new List<Structure>();

    public List<Item> Items { get; set; } = new List<Item>();

    public string OwnerId { get; set; }
}