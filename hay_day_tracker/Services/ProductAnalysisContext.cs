using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// Bundles the lookups needed to calculate product metrics: static
/// building info, current building state, and the ingredient value
/// resolver (which caches across all products). Built once and shared
/// between the Products grid and its detail panel.
/// </summary>
public class ProductAnalysisContext
{
  public Dictionary<string, ProductInfo> ProductsById { get; }
  public Dictionary<string, BuildingInfo> BuildingsById { get; }
  public Dictionary<string, BuildingState> StatesById { get; }
  public IngredientValueResolver ValueResolver { get; }

  public ProductAnalysisContext(
    List<ProductInfo> products,
    List<BuildingInfo> buildings,
    FarmStateService farmStateService)
  {
    ProductsById = products.ToDictionary(p => p.Id);
    BuildingsById = buildings.ToDictionary(b => b.Id);
    StatesById = farmStateService.Current.Buildings.ToDictionary(b => b.Id);
    ValueResolver = new IngredientValueResolver(products);
  }

  /// <summary>
  /// The current mastery level of the building that produces this
  /// product, or 0 if the building has no recorded state yet.
  /// </summary>
  public int GetMastery(ProductInfo product)
  {
    return StatesById.TryGetValue(product.BuildingId, out BuildingState? state) ? state.Mastery : 0;
  }

  /// <summary>
  /// The current owned quantity of the building that produces this
  /// product, or 0 if the building has no recorded state yet.
  /// </summary>
  public int GetQuantity(ProductInfo product)
  {
    return (StatesById.TryGetValue(product.BuildingId, out BuildingState? state) ? state.Quantity : 0) * product.Amount;
  }
}