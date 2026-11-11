namespace HayDayPlanner.Models;

public class UpgradeItemInfo
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Rarity { get; set; }

    /// <summary>
    /// Cost per single unit. Null for items that can't be bought/sold
    /// (e.g. Expansion Permits) - the shopping list skips pricing for
    /// these but still tracks how many are needed.
    /// </summary>
    public double? Price { get; set; }
}
