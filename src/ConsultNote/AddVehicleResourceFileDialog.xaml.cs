using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConsultNote.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace ConsultNote;

public partial class AddVehicleResourceFileDialog : Window
{
    private static readonly string[] FileTypes =
    [
        "견적",
        "차량설명",
        "홍보자료",
        "기타",
    ];

    private readonly IReadOnlyDictionary<string, int> _nextOrdersByFileType;
    private readonly bool _isEditMode;

    public AddVehicleResourceFileDialog(
        IReadOnlyDictionary<string, int>? nextOrdersByFileType = null,
        string? fileType = null,
        string? customFileType = null,
        int? fileOrder = null,
        string? vehicleBrand = null,
        string? vehicleName = null,
        string? fuelType = null,
        string? memo = null,
        string? sourceFilePath = null,
        bool isEditMode = false)
    {
        InitializeComponent();
        PreviewKeyDown += AddVehicleResourceFileDialog_PreviewKeyDown;
        _nextOrdersByFileType = nextOrdersByFileType ?? new Dictionary<string, int>();
        _isEditMode = isEditMode;

        FileTypeComboBox.ItemsSource = FileTypes;
        FileTypeComboBox.SelectedItem = fileType is not null && FileTypes.Contains(fileType) ? fileType : null;
        FileTypeComboBox.Text = fileType ?? FileTypes[0];
        CustomFileTypeTextBox.Text = customFileType ?? string.Empty;
        FilePathTextBox.Text = sourceFilePath ?? string.Empty;
        MemoTextBox.Text = memo ?? string.Empty;

        LoadVehicleOptions(vehicleBrand, vehicleName, fuelType);
        UpdateFileOrder();
        if (fileOrder is not null)
        {
            FileOrderTextBox.Text = fileOrder.Value.ToString();
        }

        if (_isEditMode)
        {
            TitleTextBlock.Text = "차량 자료 수정";
            FileLabelTextBlock.Visibility = Visibility.Collapsed;
            FilePickerPanel.Visibility = Visibility.Collapsed;
        }

        UpdateCustomFileTypeVisibility();
    }

    public string SourceFilePath => FilePathTextBox.Text.Trim();

    public string OriginalFileName => Path.GetFileName(SourceFilePath);

    public string FileType => TrimToNull(FileTypeComboBox.Text) ?? FileTypes[0];

    public string? CustomFileType => TrimToNull(CustomFileTypeTextBox.Text);

    public int FileOrder => int.TryParse(FileOrderTextBox.Text.Trim(), out var value) ? value : 1;

    public string? VehicleBrand => TrimToNull(VehicleBrandComboBox.Text);

    public string? VehicleName => TrimToNull(VehicleNameComboBox.Text);

    public string? FuelType => TrimToNull(FuelTypeComboBox.Text);

    public string? Memo => TrimToNull(MemoTextBox.Text);

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "지원 파일 (*.jpg;*.jpeg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf",
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        FilePathTextBox.Text = dialog.FileName;
    }

    private void VehicleBrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshVehicleNames(preferredVehicleName: null, preferredFuelType: null);
    }

    private void VehicleNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshFuelTypes(preferredFuelType: null);
    }

    private void FileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCustomFileTypeVisibility();
        UpdateFileOrder();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isEditMode && (string.IsNullOrWhiteSpace(SourceFilePath) || !File.Exists(SourceFilePath)))
        {
            MessageBox.Show("추가할 파일을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!_isEditMode && !AddCustomerFileDialog.IsSupportedFile(SourceFilePath))
        {
            MessageBox.Show(".jpg, .jpeg, .png, .pdf 파일만 추가할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(VehicleName))
        {
            MessageBox.Show("차량명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (FileType == "기타" && string.IsNullOrWhiteSpace(CustomFileType))
        {
            MessageBox.Show("기타 자료 유형명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private void AddVehicleResourceFileDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        e.Handled = true;
        SaveButton_Click(this, new RoutedEventArgs());
    }

    private void LoadVehicleOptions(string? preferredBrand, string? preferredVehicleName, string? preferredFuelType)
    {
        using var dbContext = new AppDbContext();
        var vehicles = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive)
            .OrderBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Name)
            .Select(vehicle => new VehicleOption(vehicle.Brand, vehicle.Name, vehicle.FuelTypes))
            .ToList();

        VehicleBrandComboBox.ItemsSource = vehicles
            .Select(vehicle => vehicle.Brand)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        VehicleBrandComboBox.Text = preferredBrand ?? string.Empty;
        RefreshVehicleNames(preferredVehicleName, preferredFuelType);
    }

    private void RefreshVehicleNames(string? preferredVehicleName, string? preferredFuelType)
    {
        using var dbContext = new AppDbContext();
        var selectedBrand = TrimToNull(VehicleBrandComboBox.Text);
        var vehicleNames = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive)
            .Where(vehicle => selectedBrand == null || vehicle.Brand == selectedBrand)
            .OrderBy(vehicle => vehicle.Name)
            .Select(vehicle => vehicle.Name)
            .Distinct()
            .ToList();

        VehicleNameComboBox.ItemsSource = vehicleNames;
        if (preferredVehicleName is not null)
        {
            VehicleNameComboBox.Text = preferredVehicleName;
        }

        RefreshFuelTypes(preferredFuelType);
    }

    private void RefreshFuelTypes(string? preferredFuelType)
    {
        using var dbContext = new AppDbContext();
        var selectedBrand = TrimToNull(VehicleBrandComboBox.Text);
        var selectedVehicleName = TrimToNull(VehicleNameComboBox.Text);
        var fuelTypes = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive)
            .Where(vehicle => selectedBrand == null || vehicle.Brand == selectedBrand)
            .Where(vehicle => selectedVehicleName == null || vehicle.Name == selectedVehicleName)
            .Select(vehicle => vehicle.FuelTypes)
            .AsEnumerable()
            .SelectMany(SplitFuelTypes)
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        FuelTypeComboBox.ItemsSource = fuelTypes;
        if (preferredFuelType is not null)
        {
            FuelTypeComboBox.Text = preferredFuelType;
        }
    }

    private void UpdateCustomFileTypeVisibility()
    {
        CustomFileTypePanel.Visibility = FileType == "기타" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateFileOrder()
    {
        FileOrderTextBox.Text = _nextOrdersByFileType.TryGetValue(FileType, out var nextOrder)
            ? nextOrder.ToString()
            : "1";
    }

    private static IEnumerable<string> SplitFuelTypes(string? fuelTypes)
    {
        return string.IsNullOrWhiteSpace(fuelTypes)
            ? []
            : fuelTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private sealed record VehicleOption(string? Brand, string Name, string? FuelTypes);
}
