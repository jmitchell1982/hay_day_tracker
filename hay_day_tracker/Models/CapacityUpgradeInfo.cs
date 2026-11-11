namespace HayDayPlanner.Models;

public class CapacityUpgradeInfo
{
    public string BuildingId { get; set; }

    public int StartCapacity { get; set; }

    public int MaxCapacity { get; set; }

    /// <summary>
    /// Ordered ascending by UpToCapacity. Each step says "while current
    /// capacity is below UpToCapacity, each upgrade adds Increment."
    /// E.g. Barn/Silo: [{1000, 25}, {25000, 50}].
    /// </summary>
    public List<CapacityStep> Steps { get; set; } = [];

    /// <summary>
    /// Cost of each required item for the FIRST upgrade (from
    /// StartCapacity). Cost increases by 1 per item per upgrade already
    /// performed.
    /// </summary>
    public List<UpgradeRequirement> BaseItemCosts { get; set; } = [];
}

public class CapacityStep
{
    public int UpToCapacity { get; set; }

    public int Increment { get; set; }
}
