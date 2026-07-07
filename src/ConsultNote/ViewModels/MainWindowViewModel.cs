using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using ConsultNote.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ConsultNote.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly List<CustomerItemViewModel> _allCustomers = [];
    private readonly List<VehicleOption> _vehicleOptions = [];
    private CustomerItemViewModel? _selectedCustomer;
    private FileItemViewModel? _selectedFile;
    private string? _searchText;
    private string? _selectedSortOption;
    private string? _selectedSortDirection;
    private string? _selectedVehicleBrand;
    private string? _selectedVehicleName;
    private string? _selectedFuelType;
    private string? _customerNameInput;
    private string? _customerPhoneInput;
    private string? _customerVehicleNameInput;
    private string? _customerMemoInput;
    private CustomerStatusOption? _selectedCustomerStatus;

    public MainWindowViewModel()
    {
        SortOptions.Add("등록순");
        SortOptions.Add("최근 상담순");
        SortOptions.Add("상태순");

        SortDirections.Add("오름");
        SortDirections.Add("내림");

        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.Consulting, "상담중"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.NoAnswer, "부재"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.Screening, "심사"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.ContractCompleted, "계약완료"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.Delivered, "인도완료"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.LongNoAnswer, "장기부재"));
        CustomerStatusOptions.Add(new CustomerStatusOption(CustomerStatus.Discarded, "폐기"));

        SelectedSortOption = SortOptions[0];
        SelectedSortDirection = SortDirections[1];

        SaveCustomerCommand = new RelayCommand(SaveSelectedCustomer);

        LoadVehicleOptions();
        ReloadCustomers();

        SearchText = string.Empty;
    }

    public ObservableCollection<string> SortOptions { get; } = [];

    public ObservableCollection<string> SortDirections { get; } = [];

    public ObservableCollection<CustomerItemViewModel> Customers { get; } = [];

    public ObservableCollection<string> VehicleBrands { get; } = [];

    public ObservableCollection<string> VehicleNames { get; } = [];

    public ObservableCollection<string> FuelTypes { get; } = [];

    public ObservableCollection<string> CustomerVehicleNames { get; } = [];

    public ObservableCollection<CustomerStatusOption> CustomerStatusOptions { get; } = [];

    public ICommand SaveCustomerCommand { get; }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshCustomers();
            }
        }
    }

    public string? SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
            {
                RefreshCustomers();
            }
        }
    }

    public string? SelectedSortDirection
    {
        get => _selectedSortDirection;
        set
        {
            if (SetProperty(ref _selectedSortDirection, value))
            {
                RefreshCustomers();
            }
        }
    }

    public CustomerItemViewModel? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                SelectedFile = value?.Files.FirstOrDefault();
                LoadCustomerEditor(value);
                OnPropertyChanged(nameof(SearchSummary));
            }
        }
    }

    public FileItemViewModel? SelectedFile
    {
        get => _selectedFile;
        set => SetProperty(ref _selectedFile, value);
    }

    public string? SelectedVehicleBrand
    {
        get => _selectedVehicleBrand;
        set
        {
            if (SetProperty(ref _selectedVehicleBrand, value))
            {
                RefreshVehicleNames();
            }
        }
    }

    public string? SelectedVehicleName
    {
        get => _selectedVehicleName;
        set
        {
            if (SetProperty(ref _selectedVehicleName, value))
            {
                RefreshFuelTypes();
            }
        }
    }

    public string? SelectedFuelType
    {
        get => _selectedFuelType;
        set => SetProperty(ref _selectedFuelType, value);
    }

    public string? CustomerNameInput
    {
        get => _customerNameInput;
        set => SetProperty(ref _customerNameInput, value);
    }

    public string? CustomerPhoneInput
    {
        get => _customerPhoneInput;
        set => SetProperty(ref _customerPhoneInput, value);
    }

    public string? CustomerVehicleNameInput
    {
        get => _customerVehicleNameInput;
        set => SetProperty(ref _customerVehicleNameInput, value);
    }

    public CustomerStatusOption? SelectedCustomerStatus
    {
        get => _selectedCustomerStatus;
        set => SetProperty(ref _selectedCustomerStatus, value);
    }

    public string? CustomerMemoInput
    {
        get => _customerMemoInput;
        set => SetProperty(ref _customerMemoInput, value);
    }

    public string SearchSummary
    {
        get
        {
            var totalText = _allCustomers.Count == Customers.Count
                ? $"{Customers.Count}명"
                : $"{Customers.Count}명 / 전체 {_allCustomers.Count}명";

            return string.IsNullOrWhiteSpace(SearchText)
                ? $"{totalText} · 검색어를 입력하면 즉시 좁혀집니다"
                : $"{totalText} · 이름/전화/차량/상담/고객 파일에서 검색";
        }
    }

    public void ReloadCustomers(int? selectedCustomerId = null)
    {
        selectedCustomerId ??= SelectedCustomer?.Id;

        using var dbContext = new AppDbContext();

        var customers = dbContext.Customers
            .Include(customer => customer.ConsultationLogs)
            .Include(customer => customer.Estimates)
            .Include(customer => customer.Attachments)
            .Include(customer => customer.CustomerFiles)
            .AsNoTracking()
            .ToList();

        _allCustomers.Clear();
        _allCustomers.AddRange(customers.Select(ToViewModel));
        RefreshCustomers(selectedCustomerId);
    }

    private void RefreshCustomers(int? preferredSelectedCustomerId = null)
    {
        if (SelectedSortOption is null || SelectedSortDirection is null)
        {
            return;
        }

        var selectedId = preferredSelectedCustomerId ?? SelectedCustomer?.Id;
        var query = _allCustomers.Where(MatchesSearch);

        query = SelectedSortOption switch
        {
            "최근 상담순" => query.OrderBy(customer => customer.LastConsultedAt ?? DateTime.MinValue),
            "상태순" => query.OrderBy(customer => customer.StatusOrder).ThenBy(customer => customer.Name),
            _ => query.OrderBy(customer => customer.CreatedAt),
        };

        if (SelectedSortDirection == "내림")
        {
            query = query.Reverse();
        }

        Customers.Clear();
        foreach (var customer in query)
        {
            Customers.Add(customer);
        }

        SelectedCustomer = Customers.FirstOrDefault(customer => customer.Id == selectedId)
            ?? Customers.FirstOrDefault();
        OnPropertyChanged(nameof(SearchSummary));
    }

    private void LoadCustomerEditor(CustomerItemViewModel? customer)
    {
        CustomerNameInput = customer?.Name == "-" ? string.Empty : customer?.Name;
        CustomerPhoneInput = customer?.PhoneNumber == "-" ? string.Empty : PhoneNumberFormatter.Format(customer?.PhoneNumber);
        CustomerMemoInput = customer?.MemoPreview == "-" ? string.Empty : customer?.MemoPreview;
        SelectedCustomerStatus = customer is null
            ? CustomerStatusOptions.FirstOrDefault()
            : CustomerStatusOptions.FirstOrDefault(option => option.Status == customer.Status);
    }

    private bool MatchesSearch(CustomerItemViewModel customer)
    {
        var keyword = SearchText?.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        var keywords = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return keywords.All(item => MatchesSearchKeyword(customer, item));
    }

    private static bool MatchesSearchKeyword(CustomerItemViewModel customer, string keyword)
    {
        return Contains(customer.Name, keyword)
            || PhoneNumberFormatter.Contains(customer.PhoneNumber, keyword)
            || Contains(customer.VehicleName, keyword)
            || Contains(customer.MemoPreview, keyword)
            || Contains(customer.ConditionSummary, keyword)
            || Contains(customer.SimilarEstimateSummary, keyword)
            || customer.ConsultationLogs.Any(log => Contains(log.Content, keyword))
            || customer.Files.Any(file =>
                Contains(file.FileName, keyword)
                || Contains(file.FileType, keyword)
                || Contains(file.Summary, keyword)
                || Contains(file.PreviewMeta, keyword));
    }

    public int AddCustomer(string name, string? phoneNumber, string? vehicleName)
    {
        var normalizedName = TrimToNull(name);
        if (normalizedName is null)
        {
            ShowInfo("고객 이름을 입력해주세요.");
            return 0;
        }

        var now = DateTime.Now;
        try
        {
            using var dbContext = new AppDbContext();

            var customer = new Customer
            {
                Name = normalizedName,
                PhoneNumber = PhoneNumberFormatter.Normalize(phoneNumber),
                VehicleName = TrimToNull(vehicleName),
                Status = CustomerStatus.Consulting,
                StatusChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            };

            EnsureVehicleNameExists(dbContext, customer.VehicleName, now);
            dbContext.Customers.Add(customer);
            dbContext.SaveChanges();

            LoadVehicleOptions();
            ReloadCustomers(customer.Id);
            SelectedCustomer = Customers.FirstOrDefault(item => item.Id == customer.Id) ?? Customers.FirstOrDefault();
            return customer.Id;
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
            return 0;
        }
    }

    private void SaveSelectedCustomer()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        var now = DateTime.Now;
        using var dbContext = new AppDbContext();
        var customer = dbContext.Customers.FirstOrDefault(item => item.Id == SelectedCustomer.Id);
        if (customer is null)
        {
            return;
        }

        var normalizedName = TrimToNull(CustomerNameInput);
        if (normalizedName is null)
        {
            ShowInfo("고객 이름을 입력해주세요.");
            return;
        }

        customer.Name = normalizedName;
        customer.PhoneNumber = PhoneNumberFormatter.Normalize(CustomerPhoneInput);
        customer.Memo = TrimToNull(CustomerMemoInput);
        customer.UpdatedAt = now;

        try
        {
            dbContext.SaveChanges();
            ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private static void EnsureVehicleNameExists(AppDbContext dbContext, string? vehicleName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(vehicleName))
        {
            return;
        }

        var normalizedName = vehicleName.Trim();
        var exists = dbContext.Vehicles.Any(vehicle => vehicle.Name == normalizedName);
        if (exists)
        {
            return;
        }

        dbContext.Vehicles.Add(new Vehicle
        {
            Brand = "직접입력",
            Name = normalizedName,
            Memo = "고객 관심 차량에서 추가",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    public void LoadVehicleOptions()
    {
        var previousBrand = SelectedVehicleBrand;
        var previousName = SelectedVehicleName;
        var previousFuelType = SelectedFuelType;

        using var dbContext = new AppDbContext();

        _vehicleOptions.Clear();
        _vehicleOptions.AddRange(
            dbContext.Vehicles
                .Where(vehicle => vehicle.IsActive)
                .OrderBy(vehicle => vehicle.Brand)
                .ThenBy(vehicle => vehicle.Name)
                .Select(vehicle => new VehicleOption(vehicle.Brand ?? "미지정", vehicle.Name, vehicle.FuelTypes))
                .ToList());

        VehicleBrands.Clear();
        foreach (var brand in _vehicleOptions.Select(vehicle => vehicle.Brand).Distinct())
        {
            VehicleBrands.Add(brand);
        }

        CustomerVehicleNames.Clear();
        foreach (var name in _vehicleOptions.Select(vehicle => vehicle.Name).Distinct().OrderBy(name => name))
        {
            CustomerVehicleNames.Add(name);
        }

        SelectedVehicleBrand = previousBrand is not null && VehicleBrands.Contains(previousBrand)
            ? previousBrand
            : VehicleBrands.Contains("기아") ? "기아" : VehicleBrands.FirstOrDefault();

        RefreshVehicleNames(previousName);
        RefreshFuelTypes(previousFuelType);
    }

    private void RefreshVehicleNames(string? preferredVehicleName = null)
    {
        VehicleNames.Clear();

        foreach (var name in _vehicleOptions
            .Where(vehicle => vehicle.Brand == SelectedVehicleBrand)
            .Select(vehicle => vehicle.Name)
            .Distinct()
            .OrderBy(name => name))
        {
            VehicleNames.Add(name);
        }

        var nextVehicleName = preferredVehicleName is not null && VehicleNames.Contains(preferredVehicleName)
            ? preferredVehicleName
            : SelectedVehicleName is not null && VehicleNames.Contains(SelectedVehicleName)
                ? SelectedVehicleName
                : SelectedVehicleBrand == "기아" && VehicleNames.Contains("K5")
                    ? "K5"
                    : VehicleNames.FirstOrDefault();

        SelectedVehicleName = nextVehicleName;
    }

    private void RefreshFuelTypes(string? preferredFuelType = null)
    {
        FuelTypes.Clear();

        var fuelTypes = _vehicleOptions
            .Where(vehicle => vehicle.Brand == SelectedVehicleBrand && vehicle.Name == SelectedVehicleName)
            .SelectMany(vehicle => SplitFuelTypes(vehicle.FuelTypes))
            .Distinct()
            .OrderBy(fuelType => fuelType);

        foreach (var fuelType in fuelTypes)
        {
            FuelTypes.Add(fuelType);
        }

        SelectedFuelType = preferredFuelType is not null && FuelTypes.Contains(preferredFuelType)
            ? preferredFuelType
            : SelectedFuelType is not null && FuelTypes.Contains(SelectedFuelType)
                ? SelectedFuelType
                : FuelTypes.FirstOrDefault();
    }

    private static CustomerItemViewModel ToViewModel(Customer customer)
    {
        var latestLog = customer.ConsultationLogs.OrderByDescending(log => log.CreatedAt).FirstOrDefault();
        var latestEstimate = customer.Estimates.OrderByDescending(estimate => estimate.CreatedAt).FirstOrDefault();
        var vehicleName = EmptyToDash(customer.VehicleName);

        var viewModel = new CustomerItemViewModel
        {
            Id = customer.Id,
            Name = EmptyToDash(customer.Name),
            PhoneNumber = EmptyToDash(PhoneNumberFormatter.Format(customer.PhoneNumber)),
            VehicleName = vehicleName,
            Status = customer.Status,
            StatusText = FormatStatus(customer.Status, customer.StatusChangedAt, customer.LastContactAttemptAt),
            RecentText = FormatRecent(customer.LastConsultedAt ?? latestLog?.CreatedAt),
            MemoPreview = EmptyToDash(customer.Memo),
            ConditionSummary = vehicleName,
            SimilarEstimateSummary = latestEstimate is null
                ? "참고 견적 없음"
                : $"{EmptyToDash(latestEstimate.VehicleName)} · {FormatMonthlyFee(latestEstimate.MonthlyFee)}",
            CreatedAt = customer.CreatedAt,
            LastConsultedAt = customer.LastConsultedAt ?? latestLog?.CreatedAt,
            StatusOrder = GetStatusOrder(customer.Status),
        };

        foreach (var log in customer.ConsultationLogs.OrderByDescending(log => log.CreatedAt))
        {
            viewModel.ConsultationLogs.Add(new ConsultationLogItemViewModel
            {
                CreatedAtText = log.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Content = log.Content,
            });
        }

        foreach (var customerFile in customer.CustomerFiles.OrderByDescending(file => file.CreatedAt))
        {
            viewModel.Files.Add(new FileItemViewModel
            {
                Id = customerFile.Id,
                FileName = customerFile.DisplayName,
                FilePath = customerFile.FilePath,
                FileType = GetDisplayFileType(customerFile),
                Summary = $"{GetDisplayFileType(customerFile)} · {customerFile.CreatedAt:yyyy-MM-dd}",
                PreviewTitle = customerFile.DisplayName,
                PreviewMeta = $"{GetDisplayFileType(customerFile)} · {customerFile.OriginalFileName} · {customerFile.CreatedAt:yyyy-MM-dd}",
                PreviewLabel = customerFile.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : GetDisplayFileType(customerFile),
                CreatedAt = customerFile.CreatedAt,
            });
        }

        return viewModel;
    }

    private static string FormatStatus(CustomerStatus status, DateTime? statusChangedAt, DateTime? lastContactAttemptAt)
    {
        var statusText = status switch
        {
            CustomerStatus.Consulting => "상담중",
            CustomerStatus.NoAnswer => "부재",
            CustomerStatus.Screening => "심사",
            CustomerStatus.ContractCompleted => "계약완료",
            CustomerStatus.Delivered => "인도완료",
            CustomerStatus.LongNoAnswer => "장기부재",
            CustomerStatus.Discarded => "폐기",
            _ => "상담중",
        };

        if (status is not (CustomerStatus.NoAnswer or CustomerStatus.LongNoAnswer))
        {
            return statusText;
        }

        var baseline = lastContactAttemptAt ?? statusChangedAt;
        if (baseline is null)
        {
            return statusText;
        }

        var elapsedDays = Math.Max(0, (DateTime.Today - baseline.Value.Date).Days);
        return $"{statusText} · {elapsedDays}일 경과";
    }

    private static int GetStatusOrder(CustomerStatus status)
    {
        return status switch
        {
            CustomerStatus.Consulting => 0,
            CustomerStatus.NoAnswer => 1,
            CustomerStatus.Screening => 2,
            CustomerStatus.ContractCompleted => 3,
            CustomerStatus.Delivered => 4,
            CustomerStatus.LongNoAnswer => 5,
            CustomerStatus.Discarded => 6,
            _ => 99,
        };
    }

    private static string FormatRecent(DateTime? dateTime)
    {
        if (dateTime is null)
        {
            return "상담 없음";
        }

        if (dateTime.Value.Date == DateTime.Today)
        {
            return $"최근 {dateTime:HH:mm}";
        }

        if (dateTime.Value.Date == DateTime.Today.AddDays(-1))
        {
            return "어제";
        }

        return dateTime.Value.ToString("MM-dd");
    }

    private static string FormatMonthlyFee(decimal? monthlyFee)
    {
        return monthlyFee is null ? "금액 미입력" : $"월 {monthlyFee:N0}원";
    }

    private static string EmptyToDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string GetDisplayFileType(CustomerFile customerFile)
    {
        return customerFile.FileType == "기타" && !string.IsNullOrWhiteSpace(customerFile.CustomFileType)
            ? customerFile.CustomFileType
            : customerFile.FileType;
    }

    private static bool Contains(string? source, string keyword)
    {
        return source?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static void ShowInfo(string message)
    {
        MessageBox.Show(message, "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void ShowDatabaseSaveError(Exception exception)
    {
        MessageBox.Show(
            $"저장 중 오류가 발생했습니다.\n\n{exception.Message}",
            "Consult Note",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static IEnumerable<string> SplitFuelTypes(string? fuelTypes)
    {
        return string.IsNullOrWhiteSpace(fuelTypes)
            ? []
            : fuelTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(fuelType => !string.IsNullOrWhiteSpace(fuelType));
    }

    private sealed record VehicleOption(string Brand, string Name, string? FuelTypes);

    public sealed record CustomerStatusOption(CustomerStatus Status, string DisplayName)
    {
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
