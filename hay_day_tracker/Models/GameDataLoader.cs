using System.IO;
using System.Text.Json;

namespace HayDayPlanner.Models;

/// <summary>
/// Minimal loader for gamedata.json. Deliberately narrow for now (levels
/// only) - will grow into a fuller GameDataRepository once the Machines
/// tab defines what else needs to be loaded from static game data.
/// </summary>
public static class GameDataLoader
{
    public static List<LevelInfo> LoadLevels(string path)
    {
        if (!File.Exists(path))
        {
            return new List<LevelInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("levels", out JsonElement jsonElement))
        {
            return new List<LevelInfo>();
        }

        return JsonSerializer.Deserialize<List<LevelInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<LevelInfo>();
    }
    
    public static List<ProductInfo> LoadProducts(string path)
    {
        if (!File.Exists(path))
        {
            return new List<ProductInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("products", out JsonElement productsElement))
        {
            return new List<ProductInfo>();
        }

        return JsonSerializer.Deserialize<List<ProductInfo>>(productsElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<ProductInfo>();
    }
    
    public static List<BuildingInfo> LoadBuildings(string path)
    {
        if (!File.Exists(path))
        {
            return new List<BuildingInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("buildings", out JsonElement buildingsElement))
        {
            return new List<BuildingInfo>();
        }

        return JsonSerializer.Deserialize<List<BuildingInfo>>(buildingsElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<BuildingInfo>();
    }
    
    public static List<UpgradeItemInfo> LoadUpgradeItems(string path)
    {
        if (!File.Exists(path))
        {
            return new List<UpgradeItemInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("upgradeItems", out JsonElement jsonElement))
        {
            return new List<UpgradeItemInfo>();
        }

        return JsonSerializer.Deserialize<List<UpgradeItemInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<UpgradeItemInfo>();
    }
    
    public static List<CapacityUpgradeInfo> LoadCapacityUpgrades(string path)
    {
        if (!File.Exists(path))
        {
            return new List<CapacityUpgradeInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("capacityUpgrades", out JsonElement jsonElement))
        {
            return new List<CapacityUpgradeInfo>();
        }

        return JsonSerializer.Deserialize<List<CapacityUpgradeInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<CapacityUpgradeInfo>();
    }
    
    public static List<LeveledUpgradeInfo> LoadLeveledUpgrades(string path)
    {
        if (!File.Exists(path))
        {
            return new List<LeveledUpgradeInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("leveledUpgrades", out JsonElement jsonElement))
        {
            return new List<LeveledUpgradeInfo>();
        }

        return JsonSerializer.Deserialize<List<LeveledUpgradeInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<LeveledUpgradeInfo>();
    }
    
    public static List<ShopUpgradeInfo> LoadShopUpgrades(string path)
    {
        if (!File.Exists(path))
        {
            return new List<ShopUpgradeInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("shopUpgrades", out JsonElement jsonElement))
        {
            return new List<ShopUpgradeInfo>();
        }

        return JsonSerializer.Deserialize<List<ShopUpgradeInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<ShopUpgradeInfo>();
    }
    
    public static List<PersonalTrainLevelInfo> LoadPersonalTrainLevels(string path)
    {
        if (!File.Exists(path))
        {
            return new List<PersonalTrainLevelInfo>();
        }

        string json = File.ReadAllText(path);
        JsonDocument doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("personalTrainLevels", out JsonElement jsonElement))
        {
            return new List<PersonalTrainLevelInfo>();
        }

        return JsonSerializer.Deserialize<List<PersonalTrainLevelInfo>>(jsonElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<PersonalTrainLevelInfo>();
    }
    
}
