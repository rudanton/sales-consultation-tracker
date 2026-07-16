using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ConsultNote.Data;
using ConsultNote.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace ConsultNote;

public partial class AddCustomerFileDialog : Window
{
    private static readonly string[] CommonFileTypes =
    [
        "견적",
    ];

    private static readonly string[] PersonalFileTypes =
    [
        "면허증",
        "운전경력증명서",
        "주민등록등본",
        "소득증빙서류",
    ];

    private static readonly string[] SoleProprietorFileTypes =
    [
        "면허증",
        "사업자등록증",
    ];

    private static readonly string[] CorporateFileTypes =
    [
        "법인사업자등록증앞면",
        "재무제표",
        "부가세과세표준증명원",
        "법인주주명부",
        "법인등기부등본",
        "대표님 운전면허증",
        "법인 인감증명서",
        "대표자 개인인감증명서",
    ];

    private static readonly string[] FallbackFileTypes =
    [
        .. CommonFileTypes,
        .. PersonalFileTypes,
        "사업자등록증",
        .. CorporateFileTypes,
        "계약서",
        "기타",
    ];

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
    };

    private readonly string[] _fileTypes;
    private readonly IReadOnlyDictionary<string, int> _nextOrdersByFileType;
    private readonly bool _isEditMode;
    private bool _isUpdatingVehicleOptions;

    public AddCustomerFileDialog(
        string? customerType = null,
        IReadOnlyDictionary<string, int>? nextOrdersByFileType = null,
        string? fileType = null,
        string? customFileType = null,
        int? fileOrder = null,
        string? memo = null,
        string? vehicleBrand = null,
        string? vehicleName = null,
        string? fuelType = null,
        string? capitalCompany = null,
        string? rentalCompany = null,
        string? sourceFilePath = null,
        bool isEditMode = false)
    {
        InitializeComponent();
        PreviewKeyDown += AddCustomerFileDialog_PreviewKeyDown;
        VehicleBrandComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(VehicleBrandTextBox_TextChanged));
        VehicleNameComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(VehicleNameTextBox_TextChanged));
        _isEditMode = isEditMode;
        _nextOrdersByFileType = nextOrdersByFileType ?? new Dictionary<string, int>();
        _fileTypes = GetFileTypes(customerType);
        FileTypeComboBox.ItemsSource = _fileTypes;
        FileTypeComboBox.SelectedItem = fileType is not null && _fileTypes.Contains(fileType) ? fileType : null;
        FileTypeComboBox.Text = fileType ?? _fileTypes[0];
        FilePathTextBox.Text = sourceFilePath ?? string.Empty;
        CustomFileTypeTextBox.Text = customFileType ?? string.Empty;
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
            TitleTextBlock.Text = "파일 정보 수정";
            FileLabelTextBlock.Visibility = Visibility.Collapsed;
            FilePickerPanel.Visibility = Visibility.Collapsed;
        }

        UpdateFieldVisibility();
    }

    public string SourceFilePath => FilePathTextBox.Text.Trim();

    public string OriginalFileName => Path.GetFileName(SourceFilePath);

    public string FileType => TrimToNull(FileTypeComboBox.Text) ?? _fileTypes[0];

    public string? CustomFileType => TrimToNull(CustomFileTypeTextBox.Text);

    public int FileOrder => int.TryParse(FileOrderTextBox.Text.Trim(), out var value) ? value : 1;

    public string? Memo => TrimToNull(MemoTextBox.Text);

    public string? VehicleBrand => TrimToNull(VehicleBrandComboBox.Text);

    public string? VehicleName => TrimToNull(VehicleNameComboBox.Text);

    public string? FuelType => TrimToNull(FuelTypeComboBox.Text);

    public string? CapitalCompany => null;

    public string? RentalCompany => TrimToNull(RentalCompanyTextBox.Text);

    public static bool IsSupportedFile(string filePath)
    {
        return SupportedExtensions.Contains(Path.GetExtension(filePath));
    }

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

    private void FileTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        CustomFileTypePanel.Visibility = FileType == "기타" ? Visibility.Visible : Visibility.Collapsed;
        UpdateFileOrder();
        UpdateFieldVisibility();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isEditMode && (string.IsNullOrWhiteSpace(SourceFilePath) || !File.Exists(SourceFilePath)))
        {
            MessageBox.Show("추가할 파일을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var extension = Path.GetExtension(SourceFilePath);
        if (!_isEditMode && !SupportedExtensions.Contains(extension))
        {
            MessageBox.Show(".jpg, .jpeg, .png, .pdf 파일만 추가할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (FileType == "기타" && string.IsNullOrWhiteSpace(CustomFileType))
        {
            MessageBox.Show("기타 파일 유형명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (IsEstimateFileType(FileType, CustomFileType) && string.IsNullOrWhiteSpace(VehicleName))
        {
            MessageBox.Show("견적 차량명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!int.TryParse(FileOrderTextBox.Text.Trim(), out var fileOrder) || fileOrder < 1)
        {
            MessageBox.Show("파일 순서는 1 이상의 숫자로 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private void AddCustomerFileDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        e.Handled = true;
        SaveButton_Click(this, new RoutedEventArgs());
    }

    private static string[] GetFileTypes(string? customerType)
    {
        var typedFileTypes = customerType switch
        {
            "개인" => PersonalFileTypes,
            "개인사업자" => SoleProprietorFileTypes,
            "법인사업자" => CorporateFileTypes,
            _ => null,
        };

        return typedFileTypes is null
            ? FallbackFileTypes
            : [.. CommonFileTypes, .. typedFileTypes, "기타"];
    }

    private void UpdateFileOrder()
    {
        FileOrderTextBox.Text = _nextOrdersByFileType.TryGetValue(FileType, out var nextOrder)
            ? nextOrder.ToString()
            : "1";
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
        if (_isUpdatingVehicleOptions || !EstimateMetadataPanel.IsVisible)
        {
            return;
        }

        RefreshVehicleNames(preferredVehicleName: null, preferredFuelType: null);
    }

    private void VehicleNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingVehicleOptions || !EstimateMetadataPanel.IsVisible)
        {
            return;
        }

        RefreshFuelTypes(preferredFuelType: null);
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
            .OrderBy(VehicleSort.GetBrandOrder)
            .ThenBy(VehicleSort.GetBrandSortName)
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
        var selectedBrand = TrimToNull(VehicleBrandComboBox.Text);
        var vehicleNames = dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsActive)
            .Where(vehicle => selectedBrand == null || vehicle.Brand == selectedBrand)
            .AsEnumerable()
            .GroupBy(vehicle => vehicle.Name)
            .Select(group => group.First())
            .OrderBy(vehicle => VehicleSort.GetVehicleClassOrder(vehicle.Memo))
            .ThenBy(vehicle => VehicleSort.GetPowertrainOrder(vehicle.FuelTypes))
            .ThenBy(vehicle => vehicle.Name)
            .Select(vehicle => vehicle.Name)
            .ToList();

        VehicleNameComboBox.ItemsSource = vehicleNames;
        if (preferredVehicleName is not null)
        {
            VehicleNameComboBox.Text = preferredVehicleName;
        }

        _isUpdatingVehicleOptions = false;
        RefreshFuelTypes(preferredFuelType);
    }

    private void RefreshFuelTypes(string? preferredFuelType)
    {
        _isUpdatingVehicleOptions = true;
        using var dbContext = new AppDbContext();
        var selectedBrand = TrimToNull(VehicleBrandComboBox.Text);
        var selectedVehicleName = TrimToNull(VehicleNameComboBox.Text);
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
            .SelectMany(VehicleSort.SplitFuelTypes)
            .Distinct()
            .OrderBy(VehicleSort.GetFuelTypeOrder)
            .ThenBy(value => value)
            .ToList();

        FuelTypeComboBox.ItemsSource = fuelTypes;
        if (preferredFuelType is not null)
        {
            FuelTypeComboBox.Text = preferredFuelType;
        }

        _isUpdatingVehicleOptions = false;
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

    private void UpdateFieldVisibility()
    {
        var isEstimate = IsEstimateFileType(FileType, CustomFileType);
        EstimateMetadataPanel.Visibility = isEstimate ? Visibility.Visible : Visibility.Collapsed;
        MemoPanel.Visibility = isEstimate || FileType == "기타" ? Visibility.Visible : Visibility.Collapsed;
        if (MemoPanel.Visibility != Visibility.Visible)
        {
            MemoTextBox.Text = string.Empty;
        }
    }

    private static bool IsEstimateFileType(string fileType, string? customFileType)
    {
        return string.Equals(GetDisplayFileType(fileType, customFileType), "견적", StringComparison.CurrentCultureIgnoreCase);
    }

    private static string GetDisplayFileType(string fileType, string? customFileType)
    {
        return fileType == "기타" && !string.IsNullOrWhiteSpace(customFileType)
            ? customFileType.Trim()
            : fileType.Trim();
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
