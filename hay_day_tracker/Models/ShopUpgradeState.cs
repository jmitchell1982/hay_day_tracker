namespace HayDayPlanner.Models;

public class ShopUpgradeState
{
    public string BuildingId { get; set; }

    public int SlotsLevel { get; set; }

    public int CoinsLevel { get; set; }

    public int XpReputationLevel { get; set; }

    public int ServiceTimeLevel { get; set; }

    public int OverallLevel => SlotsLevel + CoinsLevel + XpReputationLevel + ServiceTimeLevel;
}
