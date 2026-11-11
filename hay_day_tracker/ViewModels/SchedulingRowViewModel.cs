using CommunityToolkit.Mvvm.ComponentModel;

namespace HayDayPlanner.ViewModels
{
  /// <summary>
  /// Display row shared by the Uptime, Profit, and Ingredients grids.
  /// One instance per (Machine, Product) row in a SchedulingResult list.
  /// </summary>
  public sealed partial class SchedulingRowViewModel : ObservableObject
  {
    public string BuildingId { get; }
    public string BuildingName { get; }
    public string ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public double TimePerUnitMinutes { get; }
    public double TotalTimeMinutes { get; }

    /// <summary>
    /// True when this row's building appears in the SchedulingResult.Warnings
    /// list (i.e. that building went into negative remaining day-budget while
    /// funding mandatory Uptime-phase ingredient demand). Drives per-row
    /// highlighting in the DataGrid via a XAML DataTrigger.
    /// </summary>
    [ObservableProperty]
    private bool isWarning;

    /// <summary>
    /// Optional detail shown in a tooltip when IsWarning is true — e.g. the
    /// shortfall amount from SchedulingResult.Warnings for this building.
    /// </summary>
    [ObservableProperty]
    private string? warningDetail;

    public SchedulingRowViewModel(
      string buildingId,
      string buildingName,
      string productId,
      string productName,
      int quantity,
      double timePerUnitMinutes,
      double totalTimeMinutes)
    {
      BuildingId = buildingId;
      BuildingName = buildingName;
      ProductId = productId;
      ProductName = productName;
      Quantity = quantity;
      TimePerUnitMinutes = timePerUnitMinutes;
      TotalTimeMinutes = totalTimeMinutes;
    }
  }
}