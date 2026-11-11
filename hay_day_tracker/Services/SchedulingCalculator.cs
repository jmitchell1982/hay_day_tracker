using System.Collections.Generic;
using System.Linq;
using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public class ScheduleItemRow
{
    public string BuildingId { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int TimePerUnitMinutes { get; set; }
    public int TotalTimeMinutes => Quantity * TimePerUnitMinutes;
}

/// <summary>
/// A capacity shortfall raised while funding mandatory Uptime-phase
/// ingredient demand. BuildingId lets the UI highlight exactly the rows
/// belonging to the shortfall building, rather than matching on Message text.
/// </summary>
public class SchedulingWarning
{
    public string BuildingId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SchedulingResult
{
    public List<ScheduleItemRow> UptimeRows { get; set; } = [];
    public List<ScheduleItemRow> ProfitRows { get; set; } = [];
    public List<ScheduleItemRow> UptimeIngredientRows { get; set; } = [];
    public List<ScheduleItemRow> ProfitIngredientRows { get; set; } = [];
    public List<ScheduleItemRow> IngredientRows { get; set; } = [];
    public List<SchedulingWarning> Warnings { get; set; } = [];
}

/// <summary>
/// Produces the 3-phase schedule: Uptime (fixed 8h, longest item per
/// machine), Profit (greedy, one unit at a time, ranked by profit per
/// total machine-minutes consumed including ingredient production), and
/// Ingredients (whatever the other two phases end up demanding).
///
/// Every machine has a 24h budget: a fixed 8h for its Uptime pick, and
/// 960 minutes (16h) x Quantity of "day budget" shared between its own
/// Profit production and any ingredients it must supply for other
/// machines. Uptime's ingredient needs are funded first and are
/// non-negotiable; the Profit phase only adds units that fit within
/// whatever day-budget capacity remains everywhere in the chain.
/// </summary>
public static class SchedulingCalculator
{
    private const int UptimeMinutes = 8 * 60;
    private const int DayMinutesPerInstance = 16 * 60;

    public static SchedulingResult Calculate(
        FarmState state,
        List<ProductInfo> allProducts,
        List<BuildingInfo> buildings,
        ProductAnalysisContext context)
    {
        Dictionary<string, BuildingInfo> buildingsById = buildings.ToDictionary(b => b.Id);
        List<ProductInfo> unlocked = allProducts.Where(p => p.Level <= state.FarmLevel).ToList();
        Dictionary<string, List<ProductInfo>> productsByBuilding = unlocked
            .GroupBy(p => p.BuildingId)
            .ToDictionary(g => g.Key, g => g.ToList());

        Dictionary<string, double> remainingMinutes = new Dictionary<string, double>();
        foreach (KeyValuePair<string, BuildingState> kvp in context.StatesById)
        {
            if (kvp.Value.Quantity > 0)
            {
                remainingMinutes[kvp.Key] = DayMinutesPerInstance * kvp.Value.Quantity;
            }
        }

        List<SchedulingWarning> warnings = [];
        Dictionary<(string BuildingId, string ProductId), int> uptimeIngredientCommitments = new();
        Dictionary<(string BuildingId, string ProductId), int> profitIngredientCommitments = new();
        Dictionary<(string BuildingId, string ProductId), int> profitCommitments = new();
        List<ScheduleItemRow> uptimeRows = [];

        // Buildings the app treats as pure ingredient sources (Fields, etc.) -
        // excluded from Uptime and Profit entirely. They still accumulate
        // demand in ingredientCommitments (and have minutes deducted from
        // remainingMinutes) whenever some other building's chain needs what
        // they produce; they just never get their own scheduled row.
        HashSet<string> rawMaterialBuildingIds = buildingsById.Values
            .Where(building => building.IsRawMaterialOnly)
            .Select(building => building.Id)
            .ToHashSet();

        // --- Phase: Uptime picks + mandatory ingredient propagation ---
        foreach (string buildingId in remainingMinutes.Keys.ToList())
        {
            if (rawMaterialBuildingIds.Contains(buildingId))
            {
                continue;
            }

            if (!productsByBuilding.TryGetValue(buildingId, out List<ProductInfo>? candidates))
            {
                continue;
            }

            int slots = context.StatesById[buildingId].Slots;

            // Pick whichever unlocked item fills the most of the fixed 8-hour
            // window, not whichever item has the longest raw production time -
            // those aren't the same thing whenever 480 doesn't divide evenly
            // by the longest item's time (e.g. a 3h item capped at 2 units by
            // slots wastes 2h that a 2h item run 4x would have used fully).
            ProductInfo? best = null;
            int bestTime = -1;
            int bestQuantity = 0;
            int bestUtilizedMinutes = -1;

            foreach (ProductInfo candidate in candidates)
            {
                int time = ProductCalculator.GetEffectiveTimeMinutes(candidate, context.GetMastery(candidate));
                if (time <= 0)
                {
                    continue;
                }

                int quantity = System.Math.Min(UptimeMinutes / time, slots);
                if (quantity <= 0)
                {
                    continue;
                }

                int utilizedMinutes = quantity * time;

                // Maximize coverage of the 8-hour window first; among ties,
                // prefer the longer-running item, since that means fewer
                // separate plantings/collections to manage for the same
                // amount of uptime.
                if (utilizedMinutes > bestUtilizedMinutes
                    || (utilizedMinutes == bestUtilizedMinutes && time > bestTime))
                {
                    best = candidate;
                    bestTime = time;
                    bestQuantity = quantity;
                    bestUtilizedMinutes = utilizedMinutes;
                }
            }

            if (best is null)
            {
                continue;
            }

            uptimeRows.Add(new ScheduleItemRow
            {
                BuildingId = buildingId,
                MachineName = buildingsById[buildingId].Name,
                ProductId = best.Id,
                ProductName = best.Name,
                Quantity = bestQuantity,
                TimePerUnitMinutes = bestTime
            });

            ChainAnalysisResult analysis = ChainAnalyzer.Analyze(best, context);
            PropagateMandatoryDemand(analysis.Root.Children, bestQuantity, remainingMinutes, uptimeIngredientCommitments, warnings, buildingsById);
        }

        // --- Phase: Profit, greedy one-unit-at-a-time ---
        Dictionary<string, ProductInfo?> topProfitProductCache = new();
        HashSet<string> exhausted = new(rawMaterialBuildingIds);

        while (true)
        {
            string? bestBuildingId = null;
            ProductInfo? bestProduct = null;
            double bestScore = double.NegativeInfinity;
            Dictionary<string, double>? bestFootprint = null;
            ChainAnalysisResult? bestAnalysis = null;

            foreach (string buildingId in remainingMinutes.Keys)
            {
                if (exhausted.Contains(buildingId))
                {
                    continue;
                }

                if (!topProfitProductCache.TryGetValue(buildingId, out ProductInfo? product))
                {
                    product = GetTopProfitProduct(buildingId, productsByBuilding, context);
                    topProfitProductCache[buildingId] = product;
                }

                if (product is null)
                {
                    exhausted.Add(buildingId);
                    continue;
                }

                int ownTime = ProductCalculator.GetEffectiveTimeMinutes(product, context.GetMastery(product));
                if (ownTime <= 0)
                {
                    exhausted.Add(buildingId);
                    continue;
                }

                ChainAnalysisResult analysis = ChainAnalyzer.Analyze(product, context);

                Dictionary<string, double> footprint = new Dictionary<string, double> { [buildingId] = ownTime };
                AccumulateFootprintMinutes(analysis.Root.Children, footprint);

                bool feasible = footprint.All(kv =>
                    remainingMinutes.TryGetValue(kv.Key, out double remaining) && remaining >= kv.Value);

                if (!feasible)
                {
                    // Remaining capacity only ever decreases, so an
                    // infeasible addition now can never become feasible
                    // in a later round - safe to exclude permanently.
                    exhausted.Add(buildingId);
                    continue;
                }

                double profit = product.Price - context.ValueResolver.IngredientValueOf(product);
                double totalMinutes = footprint.Values.Sum();
                double score = totalMinutes > 0 ? profit / totalMinutes * Math.Log(totalMinutes) : 0;
                score = profit;

                Console.WriteLine($"{product.BuildingId}:{product.Name}|{score:F3}|{bestProduct?.BuildingId}:{bestProduct?.Name}|{bestScore:F3}");
                if (score > bestScore && profit > 0.0)
                {
                    bestScore = score;
                    bestBuildingId = buildingId;
                    bestProduct = product;
                    bestFootprint = footprint;
                    bestAnalysis = analysis;
                }
            }

            if (bestBuildingId is null || bestProduct is null || bestFootprint is null || bestAnalysis is null)
            {
                break;
            }

            foreach (KeyValuePair<string, double> kv in bestFootprint)
            {
                remainingMinutes[kv.Key] -= kv.Value;
            }

            IncrementCommitment(profitCommitments, (bestBuildingId, bestProduct.Id), 1);
            AccumulateIngredientQuantities(bestAnalysis.Root.Children, 1, profitIngredientCommitments);
        }

        List<ScheduleItemRow> profitRows = profitCommitments
            .Select(kvp => BuildRow(kvp.Key, kvp.Value, buildingsById, unlocked, context))
            .ToList();

        List<ScheduleItemRow> uptimeIngredientRows = uptimeIngredientCommitments
            .Select(kvp => BuildRow(kvp.Key, kvp.Value, buildingsById, allProducts, context))
            .ToList();

        List<ScheduleItemRow> profitIngredientRows = profitIngredientCommitments
            .Select(kvp => BuildRow(kvp.Key, kvp.Value, buildingsById, allProducts, context))
            .ToList();

        Dictionary<(string BuildingId, string ProductId), int> ingredientCommitments = new(uptimeIngredientCommitments);
        foreach (KeyValuePair<(string BuildingId, string ProductId), int> kvp in profitIngredientCommitments)
        {
            if (ingredientCommitments.ContainsKey(kvp.Key))
            {
                ingredientCommitments[kvp.Key] += kvp.Value;
            }
            else
            {
                ingredientCommitments[kvp.Key] = kvp.Value;
            }            
        }
        
        List<ScheduleItemRow> ingredientRows = ingredientCommitments
            .Select(kvp => BuildRow(kvp.Key, kvp.Value, buildingsById, allProducts, context))
            .ToList();

        return new SchedulingResult
        {
            UptimeRows = uptimeRows,
            ProfitRows = profitRows,
            IngredientRows = ingredientRows,
            UptimeIngredientRows =  uptimeIngredientRows,
            ProfitIngredientRows = profitIngredientRows,
            Warnings = warnings
        };
    }

    private static ScheduleItemRow BuildRow(
        (string BuildingId, string ProductId) key,
        int quantity,
        Dictionary<string, BuildingInfo> buildingsById,
        List<ProductInfo> products,
        ProductAnalysisContext context)
    {
        ProductInfo product = products.First(p => p.Id == key.ProductId);
        int time = ProductCalculator.GetEffectiveTimeMinutes(product, context.GetMastery(product));

        return new ScheduleItemRow
        {
            BuildingId = key.BuildingId,
            MachineName = buildingsById[key.BuildingId].Name,
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = quantity,
            TimePerUnitMinutes = time
        };
    }

    private static ProductInfo? GetTopProfitProduct(
        string buildingId,
        Dictionary<string, List<ProductInfo>> productsByBuilding,
        ProductAnalysisContext context)
    {
        if (!productsByBuilding.TryGetValue(buildingId, out List<ProductInfo>? candidates))
        {
            return null;
        }

        ProductInfo? best = null;
        double bestProfitPerMinute = double.NegativeInfinity;

        foreach (ProductInfo candidate in candidates)
        {
            int time = ProductCalculator.GetEffectiveTimeMinutes(candidate, context.GetMastery(candidate));
            if (time <= 0)
            {
                continue;
            }

            double profit = candidate.Price - context.ValueResolver.IngredientValueOf(candidate);
            double profitPerMinute = profit / time;

            if (profitPerMinute > bestProfitPerMinute)
            {
                bestProfitPerMinute = profitPerMinute;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>
    /// Deducts mandatory (non-negotiable) ingredient demand for the
    /// Uptime phase, recursively through the whole chain. Allowed to go
    /// negative - that indicates a real capacity shortfall, surfaced as
    /// a warning rather than silently failing.
    /// </summary>
    private static void PropagateMandatoryDemand(
        List<ChainNode> nodes,
        int quantityMultiplier,
        Dictionary<string, double> remainingMinutes,
        Dictionary<(string, string), int> ingredientCommitments,
        List<SchedulingWarning> warnings,
        Dictionary<string, BuildingInfo> buildingsById)
    {
        foreach (ChainNode node in nodes)
        {
            int neededQuantity = node.TotalQuantityNeeded * quantityMultiplier;
            double neededMinutes = neededQuantity * node.EffectiveTimePerUnitMinutes;
            string buildingId = node.Product.BuildingId;

            if (!remainingMinutes.ContainsKey(buildingId))
            {
                warnings.Add(new SchedulingWarning
                {
                    BuildingId = buildingId,
                    Message = $"{node.Product.Name} is needed but its building isn't owned."
                });
                remainingMinutes[buildingId] = 0;
            }

            remainingMinutes[buildingId] -= neededMinutes;

            if (remainingMinutes[buildingId] < 0)
            {
                string buildingName = buildingsById.TryGetValue(buildingId, out BuildingInfo? info) ? info.Name : buildingId;
                warnings.Add(new SchedulingWarning
                {
                    BuildingId = buildingId,
                    Message = $"Not enough {buildingName} capacity to fully supply Uptime's need for {node.Product.Name}."
                });
            }

            IncrementCommitment(ingredientCommitments, (buildingId, node.Product.Id), neededQuantity);

            PropagateMandatoryDemand(node.Children, quantityMultiplier, remainingMinutes, ingredientCommitments, warnings, buildingsById);
        }
    }

    private static void AccumulateFootprintMinutes(List<ChainNode> nodes, Dictionary<string, double> footprint)
    {
        foreach (ChainNode node in nodes)
        {
            double minutes = node.TotalQuantityNeeded * node.EffectiveTimePerUnitMinutes;
            string buildingId = node.Product.BuildingId;

            footprint.TryGetValue(buildingId, out double existing);
            footprint[buildingId] = existing + minutes;

            AccumulateFootprintMinutes(node.Children, footprint);
        }
    }

    private static void AccumulateIngredientQuantities(
        List<ChainNode> nodes,
        int quantityMultiplier,
        Dictionary<(string, string), int> ingredientCommitments)
    {
        foreach (ChainNode node in nodes)
        {
            int neededQuantity = node.TotalQuantityNeeded * quantityMultiplier;
            IncrementCommitment(ingredientCommitments, (node.Product.BuildingId, node.Product.Id), neededQuantity);
            AccumulateIngredientQuantities(node.Children, quantityMultiplier, ingredientCommitments);
        }
    }

    private static void IncrementCommitment(
        Dictionary<(string, string), int> commitments,
        (string, string) key,
        int amount)
    {
        commitments.TryGetValue(key, out int existing);
        commitments[key] = existing + amount;
    }
}