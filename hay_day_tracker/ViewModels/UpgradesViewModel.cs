using System.Collections.ObjectModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public class UpgradesViewModel
{
    public ObservableCollection<UpgradeProgressRow> ProgressRows { get; }

    public ObservableCollection<ShopUpgradeRow> ShopRows { get; }

    public ExpansionTargetViewModel Expansion { get; }

    public UpgradesViewModel(
        FarmStateService farmStateService,
        List<BuildingInfo> buildings,
        List<UpgradeItemInfo> upgradeItems,
        List<CapacityUpgradeInfo> capacityUpgrades,
        List<LeveledUpgradeInfo> leveledUpgrades,
        List<ShopUpgradeInfo> shopUpgrades,
        List<PersonalTrainLevelInfo> personalTrainLevels)
    {
        Dictionary<string, UpgradeItemInfo> itemsById = upgradeItems.ToDictionary(i => i.Id);
        Dictionary<string, BuildingInfo> buildingsById = buildings.ToDictionary(b => b.Id);
        FarmState state = farmStateService.Current;

        ObservableCollection<UpgradeProgressRow> progressRows = new ObservableCollection<UpgradeProgressRow>();

        foreach (CapacityUpgradeInfo info in capacityUpgrades.Where(x => !CapacityUpgradeCalculator.IsMaxed(x, GetCapacityAccessor(x.BuildingId, state).Get())))
        {
            (Func<int> get, Action<int> set) = GetCapacityAccessor(info.BuildingId, state);

            progressRows.Add(new UpgradeProgressRow(
                buildingsById[info.BuildingId].Name,
                "Capacity",
                get(),
                v => CapacityUpgradeCalculator.GetNextUpgradeCost(info, v),
                v =>
                {
                    set(v);
                    farmStateService.Save();
                },
                itemsById));
        }

        foreach (LeveledUpgradeInfo info in leveledUpgrades)
        {
            LeveledUpgradeState upgradeState = state.LeveledUpgrades.FirstOrDefault(l => l.BuildingId == info.BuildingId)
                ?? AddNewLeveledState(state, info.BuildingId);

            if (LeveledUpgradeCalculator.IsMaxed(upgradeState.Level, info.MaxLevel))
            {
                continue;
            }

            progressRows.Add(new UpgradeProgressRow(
                buildingsById[info.BuildingId].Name,
                "Level",
                upgradeState.Level,
                v => LeveledUpgradeCalculator.GetNextUpgradeCost(info.BaseItemCosts, v, info.MaxLevel),
                v =>
                {
                    upgradeState.Level = v;
                    farmStateService.Save();
                },
                itemsById));
        }

        if (state.PersonalTrainLevel < 19)
        {
            progressRows.Add(new UpgradeProgressRow(
                "Personal Train",
                "Level",
                state.PersonalTrainLevel,
                v => GetPersonalTrainNextCost(v, personalTrainLevels),
                v =>
                {
                    state.PersonalTrainLevel = v;
                    farmStateService.Save();
                },
                itemsById));
        }

        ProgressRows = progressRows;

        ObservableCollection<ShopUpgradeRow> shopRows = new ObservableCollection<ShopUpgradeRow>();

        foreach (ShopUpgradeInfo info in shopUpgrades)
        {
            ShopUpgradeState shopState = state.ShopUpgrades.FirstOrDefault(s => s.BuildingId == info.BuildingId)
                ?? AddNewShopState(state, info.BuildingId);

            shopRows.Add(new ShopUpgradeRow(
                buildingsById[info.BuildingId].Name,
                shopState,
                info,
                farmStateService,
                itemsById));
        }

        ShopRows = shopRows;

        Expansion = new ExpansionTargetViewModel(farmStateService);
    }

    private static LeveledUpgradeState AddNewLeveledState(FarmState state, string buildingId)
    {
        LeveledUpgradeState newState = new LeveledUpgradeState { BuildingId = buildingId, Level = 0 };
        state.LeveledUpgrades.Add(newState);
        return newState;
    }

    private static ShopUpgradeState AddNewShopState(FarmState state, string buildingId)
    {
        ShopUpgradeState newState = new ShopUpgradeState { BuildingId = buildingId };
        state.ShopUpgrades.Add(newState);
        return newState;
    }

    private static (Func<int> Get, Action<int> Set) GetCapacityAccessor(string buildingId, FarmState state)
    {
        return buildingId switch
        {
            "barn" => (() => state.BarnCapacity, v => state.BarnCapacity = v),
            "silo" => (() => state.SiloCapacity, v => state.SiloCapacity = v),
            "tackle_box" => (() => state.TackleBoxCapacity, v => state.TackleBoxCapacity = v),
            _ => throw new ArgumentException($"No capacity accessor defined for building '{buildingId}'.")
        };
    }

    private static List<UpgradeRequirement> GetPersonalTrainNextCost(
        int currentLevel,
        List<PersonalTrainLevelInfo> levels)
    {
        int nextLevel = currentLevel + 1;
        PersonalTrainLevelInfo? nextLevelInfo = levels.FirstOrDefault(l => l.Level == nextLevel);

        if (nextLevelInfo is null)
        {
            return [];
        }

        return
        [
            new UpgradeRequirement { ItemId = "refined_coal", Amount = nextLevelInfo.CoalAmount },
            new UpgradeRequirement { ItemId = "iron_bars", Amount = nextLevelInfo.IronBarsAmount },
            new UpgradeRequirement { ItemId = nextLevelInfo.PreciousMetalId, Amount = nextLevelInfo.PreciousMetalAmount }
        ];
    }
}