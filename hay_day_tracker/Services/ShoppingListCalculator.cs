using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public class ShoppingListItemResult
{
    public UpgradeItemInfo Item { get; set; } = null!;

    public int TotalAmountNeeded { get; set; }

    /// <summary>
    /// Weighted score based on TotalAmountNeeded, before accounting for
    /// inventory on hand. The row recomputes a deficit-adjusted score
    /// from this, since inventory can change after this calculation runs.
    /// </summary>
    public double PriorityScore { get; set; }
}

public static class ShoppingListCalculator
{
    public static List<ShoppingListItemResult> Calculate(
        FarmState state,
        List<CapacityUpgradeInfo> capacityUpgrades,
        List<LeveledUpgradeInfo> leveledUpgrades,
        List<ShopUpgradeInfo> shopUpgrades,
        List<PersonalTrainLevelInfo> personalTrainLevels,
        List<UpgradeItemInfo> upgradeItems,
        AppSettings settings)
    {
        Dictionary<string, UpgradeItemInfo> itemsById = upgradeItems.ToDictionary(i => i.Id);

        // Accumulates (totalAmount, weightedScore) per item as every
        // upgrade source below contributes its next-upgrade needs.
        Dictionary<string, (int Amount, double Score)> totals = new Dictionary<string, (int, double)>();

        void AddContribution(string buildingId, List<UpgradeRequirement> requirements)
        {
            double buildingWeight = GetWeight(settings.BuildingPriority, buildingId);

            foreach (UpgradeRequirement requirement in requirements)
            {
                double rarityWeight = GetWeight(settings.RarityWeight, itemsById[requirement.ItemId].Rarity);
                double contributionScore = buildingWeight * rarityWeight * requirement.Amount;

                totals.TryGetValue(requirement.ItemId, out (int Amount, double Score) existing);
                totals[requirement.ItemId] = (existing.Amount + requirement.Amount, existing.Score + contributionScore);
            }
        }

        foreach (CapacityUpgradeInfo info in capacityUpgrades)
        {
            int currentCapacity = GetCapacity(state, info.BuildingId);
            if (!CapacityUpgradeCalculator.IsMaxed(info, currentCapacity))
            {
                AddContribution(info.BuildingId, CapacityUpgradeCalculator.GetNextUpgradeCost(info, currentCapacity));
            }
        }

        foreach (LeveledUpgradeInfo info in leveledUpgrades)
        {
            LeveledUpgradeState? upgradeState = state.LeveledUpgrades.FirstOrDefault(l => l.BuildingId == info.BuildingId);
            int level = upgradeState?.Level ?? 0;

            if (!LeveledUpgradeCalculator.IsMaxed(level, info.MaxLevel))
            {
                AddContribution(info.BuildingId, LeveledUpgradeCalculator.GetNextUpgradeCost(info.BaseItemCosts, level, info.MaxLevel));
            }
        }

        foreach (ShopUpgradeInfo info in shopUpgrades)
        {
            ShopUpgradeState? shopState = state.ShopUpgrades.FirstOrDefault(s => s.BuildingId == info.BuildingId);
            int overallLevel = shopState?.OverallLevel ?? 0;

            if (!LeveledUpgradeCalculator.IsMaxed(overallLevel, info.MaxOverallLevel))
            {
                AddContribution(info.BuildingId, LeveledUpgradeCalculator.GetNextUpgradeCost(info.BaseItemCosts, overallLevel, info.MaxOverallLevel));
            }
        }

        if (!PersonalTrainCalculator.IsMaxed(state.PersonalTrainLevel))
        {
            AddContribution("personal_train", PersonalTrainCalculator.GetNextUpgradeCost(state.PersonalTrainLevel, personalTrainLevels));
        }

        if (state.ExpansionTarget is not null)
        {
            AddContribution("expansion", state.ExpansionTarget.Cost);
        }

        return totals
            .Select(kvp => new ShoppingListItemResult
            {
                Item = itemsById[kvp.Key],
                TotalAmountNeeded = kvp.Value.Amount,
                PriorityScore = kvp.Value.Score
            })
            .OrderByDescending(r => r.PriorityScore)
            .ToList();
    }

    private static double GetWeight(Dictionary<string, double> weights, string key)
    {
        return weights.TryGetValue(key, out double weight) ? weight : 1.0;
    }

    private static int GetCapacity(FarmState state, string buildingId)
    {
        return buildingId switch
        {
            "barn" => state.BarnCapacity,
            "silo" => state.SiloCapacity,
            "tackle_box" => state.TackleBoxCapacity,
            _ => 0
        };
    }
}