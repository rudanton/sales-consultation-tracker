using ConsultNote.Infrastructure;
using System.Windows;

namespace ConsultNote;

public partial class ThemeToneDialog : Window
{
    private readonly AppThemeSettings _originalSettings;
    private bool _isLoading = true;

    public ThemeToneDialog(AppThemeSettings settings)
    {
        InitializeComponent();
        Settings = settings.Clone();
        _originalSettings = settings.Clone();

        BrightnessSlider.Value = Settings.Brightness;
        WarmthSlider.Value = Settings.Warmth;
        ContrastSlider.Value = Settings.Contrast;
        _isLoading = false;
    }

    public event Action<AppThemeSettings>? PreviewChanged;

    public AppThemeSettings Settings { get; private set; }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading)
        {
            return;
        }

        Settings.Brightness = BrightnessSlider.Value;
        Settings.Warmth = WarmthSlider.Value;
        Settings.Contrast = ContrastSlider.Value;
        PreviewChanged?.Invoke(Settings);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _isLoading = true;
        Settings = AppThemeSettings.Default();
        BrightnessSlider.Value = Settings.Brightness;
        WarmthSlider.Value = Settings.Warmth;
        ContrastSlider.Value = Settings.Contrast;
        _isLoading = false;
        PreviewChanged?.Invoke(Settings);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Settings = _originalSettings.Clone();
        PreviewChanged?.Invoke(Settings);
        DialogResult = false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Settings.Clamp().Save();
        DialogResult = true;
    }
}
