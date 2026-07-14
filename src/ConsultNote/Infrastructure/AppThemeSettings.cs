using System.IO;
using System.Text.Json;

namespace ConsultNote.Infrastructure;

public sealed class AppThemeSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public double Brightness { get; set; }

    public double Warmth { get; set; }

    public double Contrast { get; set; }

    public static string SettingsPath => Path.Combine(AppPaths.SettingsDirectory, "theme.json");

    public static AppThemeSettings Default() => new();

    public static AppThemeSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return Default();
            }

            var settings = JsonSerializer.Deserialize<AppThemeSettings>(File.ReadAllText(SettingsPath), JsonOptions);
            return settings?.Clamp() ?? Default();
        }
        catch
        {
            return Default();
        }
    }

    public AppThemeSettings Clone() => new()
    {
        Brightness = Brightness,
        Warmth = Warmth,
        Contrast = Contrast,
    };

    public void Save()
    {
        Directory.CreateDirectory(AppPaths.SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Clamp(), JsonOptions));
    }

    public AppThemeSettings Clamp()
    {
        Brightness = Math.Clamp(Brightness, -18, 10);
        Warmth = Math.Clamp(Warmth, -20, 10);
        Contrast = Math.Clamp(Contrast, -12, 12);
        return this;
    }
}
