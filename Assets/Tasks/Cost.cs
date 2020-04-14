using Newtonsoft.Json;
using System.Collections.Generic;

public class Cost
{
    public Dictionary<string, int> Items = new Dictionary<string, int>();

    public static Cost AddCost(Cost cost1, Cost cost2)
    {
        var totalCost = new Cost()
        {
            Items = cost1.Items
        };
        foreach (var kvp in cost2.Items)
        {
            if (!totalCost.Items.ContainsKey(kvp.Key))
            {
                totalCost.Items.Add(kvp.Key, 0);
            }

            totalCost.Items[kvp.Key] += kvp.Value;
        }

        return totalCost;
    }

    public override string ToString()
    {
        var costString = "Cost:\n";

        if (Items.Keys.Count > 0)
        {
            foreach (var item in Items)
            {
                costString += $"{item.Key}: x{item.Value}\n";
            }
        }
        return costString;
    }
}
