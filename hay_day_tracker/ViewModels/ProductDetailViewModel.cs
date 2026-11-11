using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// One row in the "raw materials needed" summary list.
/// </summary>
public class RawMaterialRow
{
    public string Name { get; }
    public int TotalQuantity { get; }

    public RawMaterialRow(string name, int totalQuantity)
    {
        Name = name;
        TotalQuantity = totalQuantity;
    }
}

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly ProductAnalysisContext _context;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RootNodes))]
    private ChainNode? _rootNode;

    /// <summary>
    /// TreeView needs a collection to bind to; this wraps the single
    /// root node (or nothing, when no product is selected).
    /// </summary>
    public IEnumerable<ChainNode> RootNodes => RootNode is null
        ? Enumerable.Empty<ChainNode>()
        : new[] { RootNode };

    [ObservableProperty]
    private string _bottleneckDescription = string.Empty;

    public ObservableCollection<RawMaterialRow> RawMaterials { get; } = new ObservableCollection<RawMaterialRow>();

    public ProductDetailViewModel(ProductAnalysisContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Called whenever the grid's selected row changes. Passing null
    /// (nothing selected) clears the panel.
    /// </summary>
    public void ShowProduct(ProductInfo? product)
    {
        if (product is null)
        {
            RootNode = null;
            BottleneckDescription = string.Empty;
            RawMaterials.Clear();
            return;
        }

        ChainAnalysisResult analysis = ChainAnalyzer.Analyze(product, _context);

        RootNode = analysis.Root;

        BottleneckDescription = analysis.BottleneckNode == analysis.Root
            ? $"{product.Name} itself: {analysis.BottleneckNode.TimeContributionMinutes:0.#} min"
            : $"{analysis.BottleneckNode.Product.Name} "
              + $"(x{analysis.BottleneckNode.TotalQuantityNeeded}): "
              + $"{analysis.BottleneckNode.TimeContributionMinutes:0.#} min";

        RawMaterials.Clear();
        foreach ((ProductInfo rawProduct, int totalQuantity) in analysis.RawMaterials)
        {
            RawMaterials.Add(new RawMaterialRow(rawProduct.Name, totalQuantity));
        }
    }
}