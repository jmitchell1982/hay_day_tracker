using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// Shared logic for anything priced as "base cost + 1 per item per
/// level already reached" - covers Town Hall/Train Station (their own
/// single Level) and shops (their combined property level).
/// </summary>
public static class LeveledUpgradeCalculator
{
    public static List<UpgradeRequirement> GetNextUpgradeCost(
        List<UpgradeRequirement> baseItemCosts,
        int currentLevel,
        int maxLevel)
    {
        if (currentLevel >= maxLevel)
        {
            return [];
        }

        return baseItemCosts
            .Select(baseCost => new UpgradeRequirement
            {
                ItemId = baseCost.ItemId,
                Amount = baseCost.Amount + currentLevel
            })
            .ToList();
    }

    public static bool IsMaxed(int currentLevel, int maxLevel)
    {
        return currentLevel >= maxLevel;
    }
}
