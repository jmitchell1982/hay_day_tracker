namespace HayDayPlanner.Models;

public class ShopUpgradeInfo
{
    public string BuildingId { get; set; }

    /// <summary>
    /// Max combined level across all 4 properties (Slots + Coins +
    /// XpReputation + ServiceTime).
    /// </summary>
    public int MaxOverallLevel { get; set; }

    /// <summary>
    /// Cost of each required item for the first upgrade (overall level
    /// 0 to 1), regardless of which property is upgraded. Cost increases
    /// by 1 per item per overall level already reached.
    /// </summary>
    public List<UpgradeRequirement> BaseItemCosts { get; set; } = [];
}
