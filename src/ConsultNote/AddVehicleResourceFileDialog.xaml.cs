using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    private bool _isUpdatingVehicleOptions;

    public AddVehicleResourceFileDialog(
        IReadOnlyDictionary<string, int>? nextOrdersByFileType = null,
        string? fileType = null,
        string? customFileType = null,
        int? fileOrder = null,
        string? vehicleBrand = null,
        string? vehicleName = null,
        string? fuelType = null,
        string? capitalCompany = null,
        string? rentalCompany = null,
        string? memo = null,
        string? sourceFilePath = null,
        bool isEditMode = false)
    {
        InitializeComponent();
        PreviewKeyDown += AddVehicleResourceFileDialog_PreviewKeyDown;
        VehicleBrandComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(VehicleBrandTextBox_TextChanged));
        VehicleNameComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(VehicleNameTextBox_TextChanged));
        _nextOrdersByFileType = nextOrdersByFileType ?? new Dictionary<string, int>();
        _isEditMode = isEditMode;

        FileTypeComboBox.ItemsSource = FileTypes;
        FileTypeComboBox.SelectedItem = fileType is not null && FileTypes.Contains(fileType) ? fileType : null;
        FileTypeComboBox.Text = fileType ?? FileTypes[0];
        CustomFileTypeTextBox.Text = customFileType ?? string.Empty;
        FilePathTextBox.Text = sourceFilePath ?? string.Empty;
        MemoTextBox.Text = memo ?? string.Empty;
        RentalCompanyTextBox.Text = rentalCompany ?? capitalCompany ?? string.Empty;

        LoadVehicleOptions(vehicleBrand, vehicleName, fuelType);
        UpdateFileOrder();
        if (fileOrder is not null)
        {
            FileOrderTextBox.Text = fileOrder.Value.ToString();
        }

        if (_isEditMode)
        {
            TitleTextBlock.Text = "차량별 자료 수정";
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

    public string? CapitalCompany => null;

    public string? RentalCompany => TrimToNull(RentalCompanyTextBox.Text);

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
        if (_isUpdatingVehicleOptions)
        {
            return;
        }

        RefreshVehicleNames(preferredVehicleName: null, preferredFuelType: null);
    }

    private void VehicleNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingVehicleOptions)
        {
            return;
        }

        RefreshFuelTypes(preferredFuelType: null);
    }

    private void VehicleBrandTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingVehicleOptions)
        {
            return;
        }

        RefreshVehicleNames(preferredVehicleName: null, preferredFuelType: null);
    }

    private void VehicleNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingVehicleOptions)
        {
            return;
        }

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
        _isUpdatingVehicleOptions = true;
        using var dbContext = new AppDbContext();
        var brands = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive && vehicle.Brand != null)
            .Select(vehicle => vehicle.Brand!)
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        VehicleBrandComboBox.ItemsSource = brands;
        VehicleBrandComboBox.Text = preferredBrand ?? string.Empty;
        _isUpdatingVehicleOptions = false;
        RefreshVehicleNames(preferredVehicleName, preferredFuelType);
    }

    private void RefreshVehicleNames(string? preferredVehicleName, string? preferredFuelType)
    {
        _isUpdatingVehicleOptions = true;
        using var dbContext = new AppDbContext();
        var selectedBrand = GetSelectedComboText(VehicleBrandComboBox);
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
        else if (TrimToNull(VehicleNameComboBox.Text) is { } currentVehicleName && !vehicleNames.Contains(currentVehicleName))
        {
            VehicleNameComboBox.SelectedItem = null;
            VehicleNameComboBox.Text = string.Empty;
        }

        _isUpdatingVehicleOptions = false;
        RefreshFuelTypes(preferredFuelType);
    }

    private void RefreshFuelTypes(string? preferredFuelType)
    {
        _isUpdatingVehicleOptions = true;
        using var dbContext = new AppDbContext();
        var selectedBrand = GetSelectedComboText(VehicleBrandComboBox);
        var selectedVehicleName = GetSelectedComboText(VehicleNameComboBox);
        if (selectedBrand is null && selectedVehicleName is not null)
        {
            selectedBrand = TryInferBrand(dbContext, selectedVehicleName);
            if (selectedBrand is not null)
            {
                VehicleBrandComboBox.Text = selectedBrand;
            }
        }

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
        else if (TrimToNull(FuelTypeComboBox.Text) is { } currentFuelType && !fuelTypes.Contains(currentFuelType))
        {
            FuelTypeComboBox.SelectedItem = null;
            FuelTypeComboBox.Text = string.Empty;
        }

        _isUpdatingVehicleOptions = false;
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

    private static string? TryInferBrand(AppDbContext dbContext, string vehicleName)
    {
        var brands = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive && vehicle.Name == vehicleName && vehicle.Brand != null)
            .Select(vehicle => vehicle.Brand!)
            .Distinct()
            .ToList();

        return brands.Count == 1 ? brands[0] : null;
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? GetSelectedComboText(ComboBox comboBox)
    {
        return TrimToNull(comboBox.SelectedItem as string) ?? TrimToNull(comboBox.Text);
    }
}
