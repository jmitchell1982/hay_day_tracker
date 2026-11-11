using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public partial class ShoppingListRow : ObservableObject
{
  private readonly string _itemId;
  private readonly FarmState _farmState;
  private readonly FarmStateService _farmStateService;

  /// <summary>
  /// Score per unit needed, derived once from the calculator's result
  /// (TotalScore / TotalAmountNeeded). Multiplying by the live Deficit
  /// keeps the same building/rarity weighting per unit without
  /// re-running the full aggregation every time Have changes.
  /// </summary>
  private readonly double _scorePerUnit;

  public string Name { get; }

  public string Rarity { get; }

  public int TotalAmountNeeded { get; }

  public double? PricePerUnit { get; }

  public double? CostForStackOf10 => PricePerUnit.HasValue ? PricePerUnit.Value * 10 : null;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(Deficit))]
  [NotifyPropertyChangedFor(nameof(PriorityScore))]
  private int _have;

  public int Deficit => Math.Max(0, TotalAmountNeeded - Have);

  public double PriorityScore => _scorePerUnit * Deficit;

  public ShoppingListRow(ShoppingListItemResult result, FarmStateService farmStateService)
  {
    _itemId = result.Item.Id;
    _farmStateService = farmStateService;
    _farmState = farmStateService.Current;

    Name = result.Item.Name;
    Rarity = result.Item.Rarity;
    TotalAmountNeeded = result.TotalAmountNeeded;
    PricePerUnit = result.Item.Price;
    _scorePerUnit = TotalAmountNeeded > 0 ? result.PriorityScore / TotalAmountNeeded : 0;

    _have = _farmState.UpgradeItemInventory.GetValueOrDefault(_itemId, 0);
  }

  partial void OnHaveChanged(int value)
  {
    _farmState.UpgradeItemInventory[_itemId] = value;
    _farmStateService.Save();
  }
}