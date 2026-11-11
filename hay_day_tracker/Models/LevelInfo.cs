namespace HayDayPlanner.Models;

/// <summary>
/// Static per-level data from gamedata.json. XpToNextLevel matches the
/// in-game display: the amount of XP needed, counted from 0 at this level,
/// to reach the next level.
/// </summary>
public class LevelInfo
{
    public int Level { get; set; }

    public int XpToNextLevel { get; set; }
}
