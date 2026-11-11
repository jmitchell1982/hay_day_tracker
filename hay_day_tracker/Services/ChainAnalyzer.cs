using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// One node in a resolved ingredient chain. TotalQuantityNeeded already
/// accounts for quantities multiplied down the path from the root - e.g.
/// if the root needs 2 C, and C needs 3 E, TotalQuantityNeeded for E is 6.
/// </summary>
public class ChainNode
{
    public ProductInfo Product { get; }
    public int TotalQuantityNeeded { get; }
    public double ValuePerUnit { get; }
    public int EffectiveTimePerUnitMinutes { get; }
    public List<ChainNode> Children { get; }

    /// <summary>
    /// How many minutes this node's machine must spend to supply enough
    /// of this item for one unit of the root product. The node with the
    /// largest value across the whole chain is the true throughput
    /// bottleneck, since every node's machine works serially.
    /// </summary>
    public double TimeContributionMinutes => TotalQuantityNeeded * EffectiveTimePerUnitMinutes;

    public ChainNode(
        ProductInfo product,
        int totalQuantityNeeded,
        double valuePerUnit,
        int effectiveTimePerUnitMinutes,
        List<ChainNode> children)
    {
        Product = product;
        TotalQuantityNeeded = totalQuantityNeeded;
        ValuePerUnit = valuePerUnit;
        EffectiveTimePerUnitMinutes = effectiveTimePerUnitMinutes;
        Children = children;
    }
}

public class ChainAnalysisResult
{
    public ChainNode Root { get; set; } = null!;
    public ChainNode BottleneckNode { get; set; } = null!;

    /// <summary>
    /// Base ingredients (no further ingredients of their own) aggregated
    /// by product, with total quantity needed for one unit of the root
    /// product - summed across every path that uses that base item.
    /// </summary>
    public List<(ProductInfo Product, int TotalQuantity)> RawMaterials { get; set; } = new();
}

public static class ChainAnalyzer
{
    public static ChainAnalysisResult Analyze(ProductInfo rootProduct, ProductAnalysisContext context)
    {
        Dictionary<string, int> rawMaterialTotals = new Dictionary<string, int>();

        ChainNode root = BuildNode(rootProduct, quantityNeeded: 1, context, rawMaterialTotals);

        ChainNode bottleneck = FindBottleneck(root, root);

        List<(ProductInfo, int)> rawMaterials = rawMaterialTotals
            .Select(kvp => (context.ProductsById[kvp.Key], kvp.Value))
            .OrderByDescending(r => r.Value)
            .ToList();

        return new ChainAnalysisResult
        {
            Root = root,
            BottleneckNode = bottleneck,
            RawMaterials = rawMaterials
        };
    }

    private static ChainNode BuildNode(
        ProductInfo product,
        int quantityNeeded,
        ProductAnalysisContext context,
        Dictionary<string, int> rawMaterialTotals)
    {
        int mastery = context.GetMastery(product);
        int effectiveTime = ProductCalculator.GetEffectiveTimeMinutes(product, mastery);
        double valuePerUnit = context.ValueResolver.ValueOf(product);

        List<ChainNode> children = new List<ChainNode>();

        if (product.Ingredients.Count == 0)
        {
            rawMaterialTotals.TryGetValue(product.Id, out int existing);
            rawMaterialTotals[product.Id] = existing + quantityNeeded;
        }
        else
        {
            foreach (ProductIngredient ingredient in product.Ingredients)
            {
                ProductInfo childProduct = context.ProductsById[ingredient.ProductId];
                int childQuantityNeeded = quantityNeeded * ingredient.Amount;
                children.Add(BuildNode(childProduct, childQuantityNeeded, context, rawMaterialTotals));
            }
        }

        return new ChainNode(product, quantityNeeded, valuePerUnit, effectiveTime, children);
    }

    private static ChainNode FindBottleneck(ChainNode current, ChainNode best)
    {
        if (current.TimeContributionMinutes > best.TimeContributionMinutes)
        {
            best = current;
        }

        foreach (ChainNode child in current.Children)
        {
            best = FindBottleneck(child, best);
        }

        return best;
    }
}