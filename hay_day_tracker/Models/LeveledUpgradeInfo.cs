namespace HayDayPlanner.Models;

public class LeveledUpgradeInfo
{
    public string BuildingId { get; set; }

    public int MaxLevel { get; set; }

    /// <summary>
    /// Cost of each required item for the upgrade from level 0 to 1.
    /// Cost increases by 1 per item per level already reached.
    /// </summary>
    public List<UpgradeRequirement> BaseItemCosts { get; set; } = [];
}
