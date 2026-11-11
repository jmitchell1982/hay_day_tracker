using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// One row in the Machines grid. Wraps a BuildingState (mutable, persisted)
/// alongside its matching BuildingInfo (static, read-only). Edits to the
/// observable properties write back into the underlying BuildingState and
/// trigger a save through the shared FarmStateService.
/// </summary>
public partial class BuildingRow : ObservableObject
{
    private readonly BuildingState _state;
    private readonly BuildingInfo _info;
    private readonly FarmStateService _farmStateService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HoursToNextMastery))]
    private int _hours;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HoursToNextMastery))]
    private int _mastery;

    [ObservableProperty]
    private int _slots;

    [ObservableProperty]
    private int _quantity;

    public BuildingRow(BuildingState state, BuildingInfo info, FarmStateService farmStateService)
    {
        _state = state;
        _info = info;
        _farmStateService = farmStateService;

        _hours = state.Hours;
        _mastery = state.Mastery;
        _slots = state.Slots;
        _quantity = state.Quantity;
    }

    public string Id => _info.Id;

    public string Name => _info.Name;

    public string Levels => string.Join(", ", _info.Level.Distinct().Order());

    public int MaxQuantity => _info.MaxQuantity;

    public bool HasMastery => _info.MasteryTime.Count > 0;

    /// <summary>
    /// Hours still needed to reach the next mastery star, or null if the
    /// building has no mastery mechanic or is already at max mastery.
    /// </summary>
    public int? HoursToNextMastery
    {
        get
        {
            if (!HasMastery || Mastery >= _info.MasteryTime.Count)
            {
                return null;
            }

            return _info.MasteryTime[Mastery] - Hours;
        }
    }

    partial void OnHoursChanged(int value) => Save();

    partial void OnMasteryChanged(int value) => Save();

    partial void OnSlotsChanged(int value) => Save();

    partial void OnQuantityChanged(int value) => Save();

    private void Save()
    {
        _state.Hours = Hours;
        _state.Mastery = Mastery;
        _state.Slots = Slots;
        _state.Quantity = Quantity;

        _farmStateService.Save();
    }
}
