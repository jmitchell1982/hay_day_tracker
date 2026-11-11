using System.IO;
using System.Text.Json;
using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public class SettingsRepository
{
    private readonly string _path;

    public SettingsRepository(string path)
    {
        _path = path;
    }

    public AppSettings Load()
    {
        if (!File.Exists(_path))
        {
            return new AppSettings();
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(_path, json);
    }
}
