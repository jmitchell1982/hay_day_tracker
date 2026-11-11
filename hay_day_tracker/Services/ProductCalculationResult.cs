using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// Holds the calculated metrics for one product, as shown in the
/// Products tab grid. All rates (Profit/Minute, Xp/Minute) are per
/// machine-instance - Quantity only factors into MaxUnitsPerDay.
/// </summary>
public class ProductCalculationResult
{
    public double IngredientValue { get; set; }
    public double Profit { get; set; }
    public double ProfitPerUnit { get; set; }
    public int EffectiveTimeMinutes { get; set; }
    public double ProfitPerMinute { get; set; }
    public double XpPerUnit { get; set; }
    public double XpPerMinute { get; set; }
    public double ValuePerXp { get; set; }
    public double ProfitPerXp { get; set; }
    public double MaxUnitsPerDay { get; set; }
}

public static class ProductCalculator
{
    /// <summary>
    /// Computes all Products-tab metrics for one product.
    /// </summary>
    /// <param name="product">The product being evaluated.</param>
    /// <param name="mastery">
    /// The owning building's current mastery level (0 = no stars yet),
    /// used to pick base vs. mastered production time.
    /// </param>
    /// <summary>
    /// Picks base vs. mastered production time based on the owning
    /// building's current mastery level. Shared with ChainAnalyzer so
    /// every node in an ingredient chain uses the same rule.
    /// </summary>
    public static int GetEffectiveTimeMinutes(ProductInfo product, int mastery)
    {
        return product.ProductionTimeMinutes.Count != 0 ? product.ProductionTimeMinutes[mastery] : 0;
    }

    public static ProductCalculationResult Calculate(
        ProductInfo product,
        double ingredientValue,
        int mastery,
        int quantity)
    {
        int effectiveTimeMinutes = GetEffectiveTimeMinutes(product, mastery);

        double profit = product.Price - ingredientValue;
        double profitPerUnit = profit;

        double profitPerMinute = effectiveTimeMinutes > 0 ? profit / effectiveTimeMinutes : 0;
        double xpPerUnit = product.Xp;
        double xpPerMinute = effectiveTimeMinutes > 0 ? (double)product.Xp / effectiveTimeMinutes : 0;

        double valuePerXp = product.Xp > 0 ? product.Price / product.Xp : 0;
        double profitPerXp = product.Xp > 0 ? profit / product.Xp : 0;

        double maxUnitsPerDay = effectiveTimeMinutes > 0
            ? (1440.0 / effectiveTimeMinutes) * quantity
            : 0;

        return new ProductCalculationResult
        {
            IngredientValue = ingredientValue,
            Profit = profit,
            ProfitPerUnit = profitPerUnit,
            EffectiveTimeMinutes = effectiveTimeMinutes,
            ProfitPerMinute = profitPerMinute,
            XpPerUnit = xpPerUnit,
            XpPerMinute = xpPerMinute,
            ValuePerXp = valuePerXp,
            ProfitPerXp = profitPerXp,
            MaxUnitsPerDay = maxUnitsPerDay
        };
    }
}