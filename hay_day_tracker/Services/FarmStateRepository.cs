using System.IO;
using System.Text.Json;
using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

/// <summary>
/// Loads and saves farmstate.json. ViewModels call Save() after edits;
/// this class knows nothing about WPF or binding.
/// </summary>
public class FarmStateRepository
{
    private readonly string _path;

    public FarmStateRepository(string path)
    {
        _path = path;
    }

    public FarmState Load()
    {
        if (!File.Exists(_path))
        {
            return new FarmState();
        }

        string json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<FarmState>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new FarmState();
    }

    public void Save(FarmState state)
    {
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(state, options);
        File.WriteAllText(_path, json);
    }
}
