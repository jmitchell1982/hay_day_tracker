namespace HayDayPlanner.Models;

/// <summary>
/// One row of the Personal Train's level table (levels 2-19, since
/// level 1 requires no upgrade). Stored as explicit data rather than a
/// formula, matching how the main XP table is handled - observed,
/// bounded, and not worth risking a subtly wrong derivation.
/// </summary>
public class PersonalTrainLevelInfo
{
    public int Level { get; set; }

    public int CoalAmount { get; set; }

    public int IronBarsAmount { get; set; }

    public string PreciousMetalId { get; set; }

    public int PreciousMetalAmount { get; set; }
}
