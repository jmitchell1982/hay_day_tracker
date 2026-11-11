using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// Resolves the "true" value of a product's ingredients by walking the
/// ingredient DAG bottom-up. At every node, value is the max of the
/// item's own sale price or the summed value of decomposing it into its
/// own ingredients - this captures cases where an intermediate ingredient
/// is worth more sold directly than used further up the chain.
/// Results are memoized per resolver instance, since many products in
/// the chain typically share the same base ingredients.
/// </summary>
public class IngredientValueResolver
{
    private readonly Dictionary<string, ProductInfo> _productsById;
    private readonly Dictionary<string, double> _valueCache = new Dictionary<string, double>();

    public IngredientValueResolver(List<ProductInfo> allProducts)
    {
        _productsById = allProducts.ToDictionary(p => p.Id);
    }

    /// <summary>
    /// The resolved value of one unit of the given product: the greater
    /// of its own sale price or the total value of its direct ingredients
    /// (each valued the same way, recursively).
    /// </summary>
    public double ValueOf(ProductInfo product)
    {
        if (_valueCache.TryGetValue(product.Id, out double cachedValue))
        {
            return cachedValue;
        }

        double decomposedValue = 0;

        foreach (ProductIngredient ingredient in product.Ingredients)
        {
            ProductInfo ingredientProduct = _productsById[ingredient.ProductId];
            decomposedValue += ingredient.Amount * ValueOf(ingredientProduct);
        }

        double resolvedValue = product.Ingredients.Count == 0
            ? product.Price
            : Math.Max(product.Price, decomposedValue);

        _valueCache[product.Id] = resolvedValue;
        return resolvedValue;
    }

    /// <summary>
    /// The sum of ValueOf(ingredient) x amount across a product's direct
    /// ingredients - this is the figure Profit is calculated against, not
    /// the product's own resolved value.
    /// </summary>
    public double IngredientValueOf(ProductInfo product)
    {
        double total = 0;

        foreach (ProductIngredient ingredient in product.Ingredients)
        {
            ProductInfo ingredientProduct = _productsById[ingredient.ProductId];
            total += ingredient.Amount * ValueOf(ingredientProduct);
        }

        return total;
    }
}