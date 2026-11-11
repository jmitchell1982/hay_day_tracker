using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public static class PersonalTrainCalculator
{
  private const int MaxLevel = 19;

  public static bool IsMaxed(int currentLevel)
  {
    return currentLevel >= MaxLevel;
  }

  public static List<UpgradeRequirement> GetNextUpgradeCost(int currentLevel, List<PersonalTrainLevelInfo> levels)
  {
    if (IsMaxed(currentLevel))
    {
      return [];
    }

    int nextLevel = currentLevel + 1;
    PersonalTrainLevelInfo? nextLevelInfo = levels.FirstOrDefault(l => l.Level == nextLevel);

    if (nextLevelInfo is null)
    {
      return [];
    }

    return
    [
      new UpgradeRequirement { ItemId = "refined_coal", Amount = nextLevelInfo.CoalAmount },
      new UpgradeRequirement { ItemId = "iron_bars", Amount = nextLevelInfo.IronBarsAmount },
      new UpgradeRequirement { ItemId = nextLevelInfo.PreciousMetalId, Amount = nextLevelInfo.PreciousMetalAmount }
    ];
  }
}