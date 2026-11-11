using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public static class CapacityUpgradeCalculator
{
    /// <summary>
    /// How many upgrades have already been performed to reach
    /// currentCapacity from StartCapacity, walking the step schedule.
    /// </summary>
    public static int GetUpgradeCount(CapacityUpgradeInfo info, int currentCapacity)
    {
        int capacity = info.StartCapacity;
        int count = 0;

        while (capacity < currentCapacity)
        {
            int increment = GetIncrementAt(info, capacity);
            capacity += increment;
            count++;
        }

        return count;
    }

    public static bool IsMaxed(CapacityUpgradeInfo info, int currentCapacity)
    {
        return currentCapacity >= info.MaxCapacity;
    }

    /// <summary>
    /// Cost of each required item for the next upgrade, or an empty list
    /// if already at max capacity.
    /// </summary>
    public static List<UpgradeRequirement> GetNextUpgradeCost(CapacityUpgradeInfo info, int currentCapacity)
    {
        if (IsMaxed(info, currentCapacity))
        {
            return [];
        }

        int upgradeCount = GetUpgradeCount(info, currentCapacity);

        return info.BaseItemCosts
            .Select(baseCost => new UpgradeRequirement
            {
                ItemId = baseCost.ItemId,
                Amount = baseCost.Amount + upgradeCount
            })
            .ToList();
    }

    public static int GetNextCapacity(CapacityUpgradeInfo info, int currentCapacity)
    {
        if (IsMaxed(info, currentCapacity))
        {
            return currentCapacity;
        }

        int increment = GetIncrementAt(info, currentCapacity);
        return currentCapacity + increment;
    }

    private static int GetIncrementAt(CapacityUpgradeInfo info, int capacity)
    {
        foreach (CapacityStep step in info.Steps)
        {
            if (capacity < step.UpToCapacity)
            {
                return step.Increment;
            }
        }

        // Past the last defined step - use the final step's increment.
        return info.Steps[^1].Increment;
    }
}
