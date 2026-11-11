using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public partial class MachinesViewModel : ObservableObject
{
    public ObservableCollection<BuildingRow> Buildings { get; }

    public MachinesViewModel(FarmStateService farmStateService, List<BuildingInfo> buildingInfos)
    {
        FarmState state = farmStateService.Current;

        List<BuildingRow> rows = new List<BuildingRow>();

        List<string> excludedBuildings = new List<string>();
        excludedBuildings.Add("null");
        excludedBuildings.Add("field");
        excludedBuildings.Add("tree");
        excludedBuildings.Add("bush");
        excludedBuildings.Add("store");

        foreach (BuildingInfo info in buildingInfos.Where(x => !excludedBuildings.Contains(x.Id)))
        {
            BuildingState? existingState = state.Buildings.FirstOrDefault(b => b.Id == info.Id);

            if (existingState is null)
            {
                existingState = new BuildingState
                {
                    Id = info.Id,
                    Slots = 0,
                    Hours = 0,
                    Mastery = 0,
                    Quantity = 0
                };
                state.Buildings.Add(existingState);
            }

            rows.Add(new BuildingRow(existingState, info, farmStateService));
        }

        Buildings = new ObservableCollection<BuildingRow>(rows.OrderBy(x => int.Parse(x.Levels.Split(", ").First())));
    }
}
