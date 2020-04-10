using Newtonsoft.Json;
using System.Collections.Generic;

public class Cost
{
    public Dictionary<ManaColor, float> Mana = new Dictionary<ManaColor, float>();
    public Dictionary<string, int> Items = new Dictionary<string, int>();

    public static Cost AddCost(Cost cost1, Cost cost2)
    {
        var totalCost = new Cost()
        {
            Mana = ManaExtensions.AddPools(cost1.Mana, cost2.Mana),
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

        if (Mana.Keys.Count > 0)
        {
            costString += $"{Mana.GetString()}\n";
        }
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
