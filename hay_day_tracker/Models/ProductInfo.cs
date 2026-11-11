namespace HayDayPlanner.Models;

public class ProductInfo
{
  public string Id { get; set; }

  public int Amount { get; set; } = 1;
  
  public string BuildingId { get; set; }
  
  public string Name { get; set; }
  
  public int Level { get; set; }
  
  public int Xp { get; set; }
  
  public double Price { get; set; }
  
  public int? Reputation { get; set; }

  public List<int> ProductionTimeMinutes { get; set; } = [];
  
  public List<ProductIngredient> Ingredients { get; set; } = [];
}