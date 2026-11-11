namespace HayDayPlanner.Models;

/// <summary>
/// Represents the player's personal farm progress data (farmstate.json).
/// Only Farm Overview fields are included for now; Machines/Town/Upgrades
/// sections will extend this class as those tabs are built.
/// </summary>
public class FarmState
{
    public int FarmLevel { get; set; }
    
    public int Reputation { get; set; }

    /// <summary>
    /// XP earned since the last level-up, matching the in-game display
    /// (not cumulative lifetime XP).
    /// </summary>
    public int CurrentXp { get; set; }

    public int SiloCapacity { get; set; }

    public int BarnCapacity { get; set; }

    public int TackleBoxCapacity { get; set; }

    public List<BuildingState> Buildings { get; set; } = [];

    /// <summary>
    /// Current owned quantity of each upgrade item, keyed by item Id.
    /// </summary>
    public Dictionary<string, int> UpgradeItemInventory { get; set; } = new();
 
    /// <summary>
    /// Ids of upgrades already completed - excluded from the shopping
    /// list's outstanding-needs calculation.
    /// </summary>
    public List<string> CompletedUpgrades { get; set; } = [];
 
    public List<LeveledUpgradeState> LeveledUpgrades { get; set; } = [];
 
    public List<ShopUpgradeState> ShopUpgrades { get; set; } = [];
 
    /// <summary>
    /// Current Personal Train level (1-19). No list needed - there's
    /// only one Personal Train.
    /// </summary>
    public int PersonalTrainLevel { get; set; } = 1;
 
    /// <summary>
    /// Manually entered cost of whichever expansion area the player is
    /// currently targeting. Overwritten if they switch target areas -
    /// no tracking of every simultaneously-available area.
    /// </summary>
    public ExpansionTarget? ExpansionTarget { get; set; }
}
 
public class ExpansionTarget
{
    public string AreaName { get; set; } = string.Empty;
 
    public List<UpgradeRequirement> Cost { get; set; } = [];
}