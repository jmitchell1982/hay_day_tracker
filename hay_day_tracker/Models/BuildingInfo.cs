namespace HayDayPlanner.Models;

public class BuildingInfo
{
    public string Id { get; set; }
    
    public string Name { get; set; }

    public int MaxQuantity { get; set; } = 1;

    public List<int> Level { get; set; } = [];

    public List<int> MasteryTime { get; set; } = [];

    public bool IsRawMaterialOnly { get; set; } = false;
}