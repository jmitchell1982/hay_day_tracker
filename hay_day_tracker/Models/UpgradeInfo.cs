namespace HayDayPlanner.Models;

public class UpgradeInfo
{
    public string Id { get; set; }

    /// <summary>
    /// "Barn", "Silo", "Town", or "Expansion" - drives the building
    /// priority weight used in the shopping list scoring.
    /// </summary>
    public string Category { get; set; }

    public string Name { get; set; }

    public List<UpgradeRequirement> RequiredItems { get; set; } = [];
}

public class UpgradeRequirement
{
    public string ItemId { get; set; }

    public int Amount { get; set; }
}
