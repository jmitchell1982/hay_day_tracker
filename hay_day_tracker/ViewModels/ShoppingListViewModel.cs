using System.Collections.ObjectModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public class ShoppingListViewModel
{
  public ObservableCollection<ShoppingListRow> Items { get; }

  public ShoppingListViewModel(
    FarmStateService farmStateService,
    List<CapacityUpgradeInfo> capacityUpgrades,
    List<LeveledUpgradeInfo> leveledUpgrades,
    List<ShopUpgradeInfo> shopUpgrades,
    List<PersonalTrainLevelInfo> personalTrainLevels,
    List<UpgradeItemInfo> upgradeItems,
    AppSettings settings)
  {
    List<ShoppingListItemResult> results = ShoppingListCalculator.Calculate(
      farmStateService.Current,
      capacityUpgrades,
      leveledUpgrades,
      shopUpgrades,
      personalTrainLevels,
      upgradeItems,
      settings);

    Items = new ObservableCollection<ShoppingListRow>(results.Select(r => new ShoppingListRow(r, farmStateService)));
  }
}