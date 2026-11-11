using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// Backs the Scheduling tab: three grids (Uptime, Profit, Ingredients) plus a
/// building filter. Recalculates automatically whenever FarmState changes,
/// matching the rest of the app's live-recalc pattern (no manual button).
/// </summary>
public sealed partial class SchedulingViewModel : ObservableObject
{
    private readonly FarmStateService _farmStateService;
    private readonly List<ProductInfo> _allProducts;
    private readonly List<BuildingInfo> _allBuildings;

    // Built once and reused: ProductAnalysisContext wraps farmStateService
    // directly (per ProductsViewModel's construction), so it reads mastery,
    // quantities, and StatesById live off farmStateService.Current rather
    // than a snapshot - no need to rebuild it per recalculation.
    private readonly ProductAnalysisContext _context;

    // Unfiltered result from the last calculation. Kept separately from the
    // bound collections so changing the building filter is a cheap re-filter,
    // not a recalculation.
    private SchedulingResult? _lastResult;

    [ObservableProperty]
    private ObservableCollection<SchedulingRowViewModel> _uptimeRows = new();

    [ObservableProperty]
    private ObservableCollection<SchedulingRowViewModel> _profitRows = new();

    [ObservableProperty]
    private ObservableCollection<SchedulingRowViewModel> _ingredientRows = new();

    /// <summary>
    /// "All Buildings" plus one entry per owned building (Quantity > 0).
    /// Bind the Uptime/Profit/Ingredients ComboBox SelectedItem to
    /// SelectedBuildingFilter against this list.
    /// </summary>
    public ObservableCollection<BuildingFilterOption> BuildingFilterOptions { get; } = new();

    [ObservableProperty]
    private BuildingFilterOption _selectedBuildingFilter;

    /// <summary>
    /// "Full Schedule"
    /// </summary>
    public ObservableCollection<string> ScheduleFilterOptions { get; } = new();
    
    [ObservableProperty]
    private string _selectedScheduleFilter;

    public SchedulingViewModel(
        FarmStateService farmStateService,
        List<ProductInfo> allProducts,
        List<BuildingInfo> allBuildings)
    {
        _farmStateService = farmStateService;
        _allProducts = allProducts;
        _allBuildings = allBuildings;
        _context = new ProductAnalysisContext(allProducts, allBuildings, farmStateService);

        BuildingFilterOption allOption = BuildingFilterOption.AllBuildings;
        BuildingFilterOptions.Add(allOption);
        _selectedBuildingFilter = allOption;
        
        ScheduleFilterOptions.Add("Full Schedule");
        ScheduleFilterOptions.Add("Uptime Only");
        ScheduleFilterOptions.Add("Profit Only");
        _selectedScheduleFilter = "Full Schedule";

        RebuildBuildingFilterOptions();

        // Recalculate whenever any tab's edits touch the shared FarmState.
        _farmStateService.FarmStateChanged += OnFarmStateChanged;

        Recalculate();
    }

    private void OnFarmStateChanged(object? sender, EventArgs e)
    {
        RebuildBuildingFilterOptions();
        Recalculate();
    }

    private void RebuildBuildingFilterOptions()
    {
        BuildingFilterOption previousSelection = SelectedBuildingFilter;

        BuildingFilterOptions.Clear();
        BuildingFilterOptions.Add(BuildingFilterOption.AllBuildings);

        IEnumerable<BuildingState> ownedBuildings = _farmStateService.Current.Buildings
            .Where(buildingState => buildingState.Quantity > 0)
            .OrderBy(buildingState => _allBuildings.Single(x => x.Id == buildingState.Id).Level.First())
            .ToList();

        foreach (BuildingState buildingState in ownedBuildings)
        {
            BuildingInfo? info = _allBuildings.FirstOrDefault(building => building.Id == buildingState.Id);
            if (info is not null)
            {
                BuildingFilterOptions.Add(new BuildingFilterOption(info.Id, info.Name));
            }
        }

        // Preserve the user's filter selection across recalculation if that
        // building still exists; otherwise fall back to "All Buildings".
        BuildingFilterOption? stillPresent = BuildingFilterOptions
            .FirstOrDefault(option => option.BuildingId == previousSelection.BuildingId);
        SelectedBuildingFilter = stillPresent ?? BuildingFilterOption.AllBuildings;
    }

    partial void OnSelectedBuildingFilterChanged(BuildingFilterOption value)
    {
        ApplyFilter();
    }

    partial void OnSelectedScheduleFilterChanged(string value)
    {
        ApplyFilter();
    }

    private void Recalculate()
    {
        _lastResult = SchedulingCalculator.Calculate(_farmStateService.Current, _allProducts, _allBuildings, _context);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (_lastResult is null)
        {
            return;
        }

        HashSet<string> warningBuildingIds = _lastResult.Warnings
            .Select(warning => warning.BuildingId)
            .ToHashSet();

        bool showAllBuildings = SelectedBuildingFilter?.BuildingId is null;
        bool showAllSchedules = SelectedScheduleFilter == "Full Schedule";

        // Uptime and Profit: filter directly by the row's own building.
        UptimeRows = BuildRowCollection(
            _lastResult.UptimeRows.Where(row => (showAllSchedules || SelectedScheduleFilter == "Uptime Only") && (showAllBuildings || row.BuildingId == SelectedBuildingFilter.BuildingId)),
            warningBuildingIds,
            _lastResult.Warnings);

        ProfitRows = BuildRowCollection(
            _lastResult.ProfitRows.Where(row => (showAllSchedules || SelectedScheduleFilter == "Profit Only") && (showAllBuildings || row.BuildingId == SelectedBuildingFilter.BuildingId)),
            warningBuildingIds,
            _lastResult.Warnings);

        IEnumerable<ScheduleItemRow> selectedScheduleRows =
            SelectedScheduleFilter switch
            {
                "Uptime Only" => _lastResult.UptimeIngredientRows,
                "Profit Only" => _lastResult.ProfitIngredientRows,
                _ => _lastResult.IngredientRows
            };
        
        // Ingredients: filter by "is this product needed somewhere in the
        // ingredient chain of one of the selected building's unlocked
        // products", via ChainAnalyzer.
        if (showAllBuildings)
        {
            IngredientRows = BuildRowCollection(selectedScheduleRows, warningBuildingIds, _lastResult.Warnings);
        }
        else
        {
            HashSet<string> neededProductIds = new();

            IEnumerable<ProductInfo> buildingProducts = _allProducts
                .Where(product => product.BuildingId == SelectedBuildingFilter.BuildingId
                                  && product.Level <= _farmStateService.Current.FarmLevel);

            foreach (ProductInfo product in buildingProducts)
            {
                ChainAnalysisResult analysis = ChainAnalyzer.Analyze(product, _context);
                CollectDescendantProductIds(analysis.Root.Children, neededProductIds);
            }

            IngredientRows = BuildRowCollection(
                selectedScheduleRows.Where(row => neededProductIds.Contains(row.ProductId)),
                warningBuildingIds,
                _lastResult.Warnings);
        }
    }

    private static void CollectDescendantProductIds(List<ChainNode> nodes, HashSet<string> result)
    {
        foreach (ChainNode node in nodes)
        {
            result.Add(node.Product.Id);
            CollectDescendantProductIds(node.Children, result);
        }
    }

    private static ObservableCollection<SchedulingRowViewModel> BuildRowCollection(
        IEnumerable<ScheduleItemRow> rows,
        HashSet<string> warningBuildingIds,
        IReadOnlyList<SchedulingWarning> warnings)
    {
        ObservableCollection<SchedulingRowViewModel> collection = new();

        foreach (ScheduleItemRow row in rows)
        {
            SchedulingRowViewModel rowViewModel = new(
                buildingId: row.BuildingId,
                buildingName: row.MachineName,
                productId: row.ProductId,
                productName: row.ProductName,
                quantity: row.Quantity,
                timePerUnitMinutes: row.TimePerUnitMinutes,
                totalTimeMinutes: row.TotalTimeMinutes);

            if (warningBuildingIds.Contains(row.BuildingId))
            {
                rowViewModel.IsWarning = true;
                SchedulingWarning? warning = warnings.FirstOrDefault(w => w.BuildingId == row.BuildingId);
                rowViewModel.WarningDetail = warning?.Message;
            }

            collection.Add(rowViewModel);
        }

        return collection;
    }
}

/// <summary>
/// One entry in the building filter ComboBox. BuildingId is null for the
/// "All Buildings" option.
/// </summary>
public sealed record BuildingFilterOption(string? BuildingId, string DisplayName)
{
    public static BuildingFilterOption AllBuildings { get; } = new(null, "All Buildings");
}