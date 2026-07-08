using System.IO;
using System.Text.Json;
using System.Windows;

namespace ConsultNote.Infrastructure;

public static class WindowPlacementStore
{
    private static readonly string FilePath = Path.Combine(AppPaths.SettingsDirectory, "window-placement.json");

    public static void Apply(Window window)
    {
        var placement = Load();
        if (placement is null)
        {
            return;
        }

        var width = Math.Max(window.MinWidth, placement.Width);
        var height = Math.Max(window.MinHeight, placement.Height);
        if (!IsFinitePositive(width) || !IsFinitePositive(height))
        {
            return;
        }

        window.Width = width;
        window.Height = height;
        window.Left = Clamp(placement.Left, SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 80);
        window.Top = Clamp(placement.Top, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 80);
        window.WindowState = placement.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    public static void Save(Window window)
    {
        var bounds = window.WindowState == WindowState.Normal
            ? new Rect(window.Left, window.Top, window.Width, window.Height)
            : window.RestoreBounds;

        if (!IsFinitePositive(bounds.Width) || !IsFinitePositive(bounds.Height))
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.SettingsDirectory);
        var placement = new WindowPlacement
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Width = bounds.Width,
            Height = bounds.Height,
            IsMaximized = window.WindowState == WindowState.Maximized,
        };

        File.WriteAllText(FilePath, JsonSerializer.Serialize(placement, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static WindowPlacement? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            return JsonSerializer.Deserialize<WindowPlacement>(File.ReadAllText(FilePath));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsFinitePositive(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    private sealed class WindowPlacement
    {
        public double Left { get; set; }

        public double Top { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool IsMaximized { get; set; }
    }
}
