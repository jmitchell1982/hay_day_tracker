using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public partial class ShopUpgradeRow : ObservableObject
{
    private readonly ShopUpgradeState _state;
    private readonly ShopUpgradeInfo _info;
    private readonly FarmStateService _farmStateService;
    private readonly Dictionary<string, UpgradeItemInfo> _itemsById;

    public string Name { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallLevel))]
    [NotifyPropertyChangedFor(nameof(NextCostDisplay))]
    private int _slotsLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallLevel))]
    [NotifyPropertyChangedFor(nameof(NextCostDisplay))]
    private int _coinsLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallLevel))]
    [NotifyPropertyChangedFor(nameof(NextCostDisplay))]
    private int _xpReputationLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverallLevel))]
    [NotifyPropertyChangedFor(nameof(NextCostDisplay))]
    private int _serviceTimeLevel;

    public int OverallLevel => SlotsLevel + CoinsLevel + XpReputationLevel + ServiceTimeLevel;

    public string NextCostDisplay
    {
        get
        {
            List<UpgradeRequirement> cost = LeveledUpgradeCalculator.GetNextUpgradeCost(
                _info.BaseItemCosts,
                OverallLevel,
                _info.MaxOverallLevel);

            return string.Join(", ", cost.Select(c => $"{c.Amount} {_itemsById[c.ItemId].Name}"));
        }
    }

    public ShopUpgradeRow(
        string name,
        ShopUpgradeState state,
        ShopUpgradeInfo info,
        FarmStateService farmStateService,
        Dictionary<string, UpgradeItemInfo> itemsById)
    {
        Name = name;
        _state = state;
        _info = info;
        _farmStateService = farmStateService;
        _itemsById = itemsById;

        _slotsLevel = state.SlotsLevel;
        _coinsLevel = state.CoinsLevel;
        _xpReputationLevel = state.XpReputationLevel;
        _serviceTimeLevel = state.ServiceTimeLevel;
    }

    partial void OnSlotsLevelChanged(int value) => Save();

    partial void OnCoinsLevelChanged(int value) => Save();

    partial void OnXpReputationLevelChanged(int value) => Save();

    partial void OnServiceTimeLevelChanged(int value) => Save();

    private void Save()
    {
        _state.SlotsLevel = SlotsLevel;
        _state.CoinsLevel = CoinsLevel;
        _state.XpReputationLevel = XpReputationLevel;
        _state.ServiceTimeLevel = ServiceTimeLevel;

        _farmStateService.Save();
    }
}