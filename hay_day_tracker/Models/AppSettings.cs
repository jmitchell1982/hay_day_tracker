namespace HayDayPlanner.Models;

public class AppSettings
{
  /// <summary>
  /// Relative importance of each building's upgrades, keyed by
  /// building Id (plus "expansion" for the manual expansion target).
  /// Higher = more important. Tunable by the player, not game data.
  /// </summary>
  public Dictionary<string, double> BuildingPriority { get; set; } = new();

  /// <summary>
  /// Relative weight per rarity tier (e.g. "Common", "Uncommon",
  /// "Rare"). Higher = rarer/more valuable to prioritize.
  /// </summary>
  public Dictionary<string, double> RarityWeight { get; set; } = new();
}