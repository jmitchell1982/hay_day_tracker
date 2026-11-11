using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

/// <summary>
/// One row in the Products grid. Read-only - this tab displays
/// calculated data, it doesn't edit farm state. Keeps a reference to the
/// underlying ProductInfo for the detail panel (built separately).
/// </summary>
public class ProductRow
{
  public ProductInfo Product { get; }

  public string Name => Product.Name;
  
  public int Level => Product.Level;

  public string MachineName { get; }

  public double Price => Product.Price;

  public double IngredientValue { get; }

  public double Profit { get; }

  public double ProfitPerUnit { get; }

  public int EffectiveTimeMinutes { get; }

  public double ProfitPerMinute { get; }

  public double XpPerUnit { get; }

  public double XpPerMinute { get; }

  public double ValuePerXp { get; }

  public double ProfitPerXp { get; }

  public double MaxUnitsPerDay { get; }

  public ProductRow(ProductInfo product, string machineName, ProductCalculationResult result)
  {
    Product = product;
    MachineName = machineName;

    IngredientValue = result.IngredientValue;
    Profit = result.Profit;
    ProfitPerUnit = result.ProfitPerUnit;
    EffectiveTimeMinutes = result.EffectiveTimeMinutes;
    ProfitPerMinute = result.ProfitPerMinute;
    XpPerUnit = result.XpPerUnit;
    XpPerMinute = result.XpPerMinute;
    ValuePerXp = result.ValuePerXp;
    ProfitPerXp = result.ProfitPerXp;
    MaxUnitsPerDay = result.MaxUnitsPerDay;
  }
}