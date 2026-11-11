using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// Manual entry for whichever expansion area the player is currently
/// targeting. Hardcoded to the 4 purchasable expansion supply types
/// (Expansion Permits are excluded - not purchasable) rather than a
/// dynamic item list, since this set never changes.
/// </summary>
public partial class ExpansionTargetViewModel : ObservableObject
{
    private const string LandDeedId = "land_deed";
    private const string MalletId = "mallet";
    private const string MapPieceId = "map_piece";
    private const string MarkerStakeId = "marker_stake";

    private readonly FarmState _state;
    private readonly FarmStateService _farmStateService;

    [ObservableProperty]
    private string _areaName = string.Empty;

    [ObservableProperty]
    private int _landDeedAmount;

    [ObservableProperty]
    private int _malletAmount;

    [ObservableProperty]
    private int _mapPieceAmount;

    [ObservableProperty]
    private int _markerStakeAmount;

    public ExpansionTargetViewModel(FarmStateService farmStateService)
    {
        _farmStateService = farmStateService;
        _state = farmStateService.Current;

        ExpansionTarget? target = _state.ExpansionTarget;
        if (target is not null)
        {
            _areaName = target.AreaName;
            _landDeedAmount = GetAmount(target, LandDeedId);
            _malletAmount = GetAmount(target, MalletId);
            _mapPieceAmount = GetAmount(target, MapPieceId);
            _markerStakeAmount = GetAmount(target, MarkerStakeId);
        }
    }

    partial void OnAreaNameChanged(string value) => Save();

    partial void OnLandDeedAmountChanged(int value) => Save();

    partial void OnMalletAmountChanged(int value) => Save();

    partial void OnMapPieceAmountChanged(int value) => Save();

    partial void OnMarkerStakeAmountChanged(int value) => Save();

    private static int GetAmount(ExpansionTarget target, string itemId)
    {
        return target.Cost.FirstOrDefault(c => c.ItemId == itemId)?.Amount ?? 0;
    }

    private void Save()
    {
        List<UpgradeRequirement> cost = [];
        if (LandDeedAmount > 0) cost.Add(new UpgradeRequirement { ItemId = LandDeedId, Amount = LandDeedAmount });
        if (MalletAmount > 0) cost.Add(new UpgradeRequirement { ItemId = MalletId, Amount = MalletAmount });
        if (MapPieceAmount > 0) cost.Add(new UpgradeRequirement { ItemId = MapPieceId, Amount = MapPieceAmount });
        if (MarkerStakeAmount > 0) cost.Add(new UpgradeRequirement { ItemId = MarkerStakeId, Amount = MarkerStakeAmount });

        _state.ExpansionTarget = new ExpansionTarget { AreaName = AreaName, Cost = cost };
        _farmStateService.Save();
    }
}