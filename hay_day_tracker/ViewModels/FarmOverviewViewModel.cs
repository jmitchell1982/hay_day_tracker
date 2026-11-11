using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// Backs the Farm Overview tab. Every editable property auto-saves to
/// farmstate.json on change via the partial On&lt;Property&gt;Changed hooks
/// that CommunityToolkit.Mvvm generates for [ObservableProperty] fields.
/// </summary>
public partial class FarmOverviewViewModel : ObservableObject
{
    private readonly FarmStateService _farmStateService;
    private readonly List<LevelInfo> _levels;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(XpNeededForNextLevel))]
    private int _farmLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(XpNeededForNextLevel))]
    private int _currentXp;

    [ObservableProperty]
    private int _siloCapacity;

    [ObservableProperty]
    private int _barnCapacity;

    [ObservableProperty]
    private int _tackleBoxCapacity;

    public FarmOverviewViewModel(FarmStateService farmStateService, List<LevelInfo> levels)
    {
        _farmStateService = farmStateService;
        _levels = levels;

        FarmState state = _farmStateService.Current;
        _farmLevel = state.FarmLevel;
        _currentXp = state.CurrentXp;
        _siloCapacity = state.SiloCapacity;
        _barnCapacity = state.BarnCapacity;
        _tackleBoxCapacity = state.TackleBoxCapacity;
    }

    /// <summary>
    /// XP still needed to reach the next level, matching the in-game
    /// "since last level-up" display. Returns 0 if the level isn't found
    /// in gamedata (e.g. max level reached).
    /// </summary>
    public int XpNeededForNextLevel
    {
        get
        {
            LevelInfo? levelInfo = _levels.FirstOrDefault(l => l.Level == FarmLevel);
            if (levelInfo is null)
            {
                return 0;
            }

            return levelInfo.XpToNextLevel - CurrentXp;
        }
    }

    // CommunityToolkit.Mvvm generates OnFarmLevelChanged, OnCurrentXpChanged,
    // etc. automatically because the backing fields are [ObservableProperty].
    // Each partial method below hooks into that generated call to auto-save.

    partial void OnFarmLevelChanged(int value) => Save();

    partial void OnCurrentXpChanged(int value) => Save();

    partial void OnSiloCapacityChanged(int value) => Save();

    partial void OnBarnCapacityChanged(int value) => Save();

    partial void OnTackleBoxCapacityChanged(int value) => Save();

    private void Save()
    {
        FarmState state = _farmStateService.Current;
        state.FarmLevel = FarmLevel;
        state.CurrentXp = CurrentXp;
        state.SiloCapacity = SiloCapacity;
        state.BarnCapacity = BarnCapacity;
        state.TackleBoxCapacity = TackleBoxCapacity;
        
        _farmStateService.Save();
    }
}
