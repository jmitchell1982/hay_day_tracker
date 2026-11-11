using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// Displays and edits one "progress value -> next cost" upgrade. The
/// actual cost/maxed rules are injected as delegates, since Capacity
/// (Barn/Silo/Tackle Box), simple Level (Town Hall/Train Station), and
/// the Personal Train's level-table lookup all share this same shape
/// but compute cost differently.
/// </summary>
public partial class UpgradeProgressRow : ObservableObject
{
    private readonly Dictionary<string, UpgradeItemInfo> _itemsById;
    private readonly Func<int, List<UpgradeRequirement>> _getNextCost;
    private readonly Action<int> _onValueChanged;

    public string Name { get; }

    /// <summary>"Capacity" or "Level" - shown as a column label in the grid.</summary>
    public string ValueLabel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextCostDisplay))]
    private int _currentValue;

    public string NextCostDisplay
    {
        get
        {
            List<UpgradeRequirement> cost = _getNextCost(CurrentValue);
            return string.Join(", ", cost.Select(c => $"{c.Amount} {_itemsById[c.ItemId].Name}"));
        }
    }

    public UpgradeProgressRow(
        string name,
        string valueLabel,
        int initialValue,
        Func<int, List<UpgradeRequirement>> getNextCost,
        Action<int> onValueChanged,
        Dictionary<string, UpgradeItemInfo> itemsById)
    {
        Name = name;
        ValueLabel = valueLabel;
        _currentValue = initialValue;
        _getNextCost = getNextCost;
        _onValueChanged = onValueChanged;
        _itemsById = itemsById;
    }

    partial void OnCurrentValueChanged(int value) => _onValueChanged(value);
}