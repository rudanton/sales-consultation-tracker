using System.Collections.ObjectModel;
using System.Windows.Input;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
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

    public MainWindowViewModel()
    {
        SortOptions.Add("등록순");
        SortOptions.Add("최근 상담순");
        SortOptions.Add("상태순");

        SortDirections.Add("오름");
        SortDirections.Add("내림");

        SelectedSortOption = SortOptions[0];
        SelectedSortDirection = SortDirections[1];

        AddCustomerCommand = new RelayCommand(AddCustomer);

        EnsureSampleCustomers();
        LoadVehicleOptions();
        LoadCustomers();

        SearchText = string.Empty;
    }

    public ObservableCollection<string> SortOptions { get; } = [];

    public ObservableCollection<string> SortDirections { get; } = [];

    public ObservableCollection<CustomerItemViewModel> Customers { get; } = [];

    public ObservableCollection<string> VehicleBrands { get; } = [];

    public ObservableCollection<string> VehicleNames { get; } = [];

    public ObservableCollection<string> FuelTypes { get; } = [];

    public ICommand AddCustomerCommand { get; }

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

    public string SearchSummary
    {
        get
        {
            var totalText = _allCustomers.Count == Customers.Count
                ? $"{Customers.Count}명"
                : $"{Customers.Count}명 / 전체 {_allCustomers.Count}명";

            return string.IsNullOrWhiteSpace(SearchText)
                ? $"{totalText} · 검색어를 입력하면 즉시 좁혀집니다"
                : $"{totalText} · 이름/전화/차량/상담/견적에서 검색";
        }
    }

    private void LoadCustomers()
    {
        using var dbContext = new AppDbContext();

        var customers = dbContext.Customers
            .Include(customer => customer.ConsultationLogs)
            .Include(customer => customer.Estimates)
            .Include(customer => customer.Attachments)
            .AsNoTracking()
            .ToList();

        _allCustomers.Clear();
        _allCustomers.AddRange(customers.Select(ToViewModel));
        RefreshCustomers();
    }

    private void RefreshCustomers()
    {
        if (SelectedSortOption is null || SelectedSortDirection is null)
        {
            return;
        }

        var selectedId = SelectedCustomer?.Id;
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

    private bool MatchesSearch(CustomerItemViewModel customer)
    {
        var keyword = SearchText?.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        return Contains(customer.Name, keyword)
            || Contains(customer.PhoneNumber, keyword)
            || Contains(customer.VehicleName, keyword)
            || Contains(customer.MemoPreview, keyword)
            || Contains(customer.CompanyDbReference, keyword)
            || customer.ConsultationLogs.Any(log => Contains(log.Content, keyword))
            || customer.Files.Any(file => Contains(file.FileName, keyword) || Contains(file.Summary, keyword));
    }

    private void AddCustomer()
    {
        var now = DateTime.Now;
        using var dbContext = new AppDbContext();
        var nextNumber = dbContext.Customers.Count() + 1;

        var customer = new Customer
        {
            Name = $"신규 고객 {nextNumber}",
            PhoneNumber = "010-",
            VehicleName = SelectedVehicleName,
            CompanyDbReference = string.Empty,
            Memo = "새 고객 메모를 입력하세요.",
            Status = CustomerStatus.Consulting,
            StatusChangedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Customers.Add(customer);
        dbContext.SaveChanges();

        LoadCustomers();
        SelectedCustomer = Customers.FirstOrDefault(item => item.Id == customer.Id) ?? Customers.FirstOrDefault();
    }

    private void LoadVehicleOptions()
    {
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

        SelectedVehicleBrand = VehicleBrands.Contains("기아") ? "기아" : VehicleBrands.FirstOrDefault();
        SelectedVehicleName = VehicleNames.Contains("K5") ? "K5" : VehicleNames.FirstOrDefault();
    }

    private void RefreshVehicleNames()
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

        SelectedVehicleName = SelectedVehicleName is not null && VehicleNames.Contains(SelectedVehicleName)
            ? SelectedVehicleName
            : VehicleNames.FirstOrDefault();
    }

    private void RefreshFuelTypes()
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

        SelectedFuelType = SelectedFuelType is not null && FuelTypes.Contains(SelectedFuelType)
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
            PhoneNumber = EmptyToDash(customer.PhoneNumber),
            VehicleName = vehicleName,
            StatusText = FormatStatus(customer.Status, customer.StatusChangedAt, customer.LastContactAttemptAt),
            RecentText = FormatRecent(customer.LastConsultedAt ?? latestLog?.CreatedAt),
            MemoPreview = EmptyToDash(customer.Memo),
            CompanyDbReference = EmptyToDash(customer.CompanyDbReference),
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

        foreach (var estimate in customer.Estimates.OrderByDescending(estimate => estimate.CreatedAt))
        {
            viewModel.Files.Add(new FileItemViewModel
            {
                FileName = estimate.OriginalFileName,
                Summary = $"{EmptyToDash(estimate.VehicleName)} · {FormatMonthlyFee(estimate.MonthlyFee)}",
                PreviewTitle = estimate.OriginalFileName,
                PreviewMeta = $"{EmptyToDash(estimate.VehicleName)} · {FormatMonthlyFee(estimate.MonthlyFee)} · {EmptyToDash(estimate.Status)}",
                PreviewLabel = "견적 이미지",
            });
        }

        foreach (var attachment in customer.Attachments.OrderByDescending(attachment => attachment.CreatedAt))
        {
            viewModel.Files.Add(new FileItemViewModel
            {
                FileName = attachment.OriginalFileName,
                Summary = EmptyToDash(attachment.FileType),
                PreviewTitle = attachment.OriginalFileName,
                PreviewMeta = $"{EmptyToDash(attachment.FileType)} · {attachment.CreatedAt:yyyy-MM-dd}",
                PreviewLabel = "첨부파일",
            });
        }

        return viewModel;
    }

    private static void EnsureSampleCustomers()
    {
        using var dbContext = new AppDbContext();
        if (dbContext.Customers.Any())
        {
            return;
        }

        var today = DateTime.Today.AddHours(10).AddMinutes(30);
        var yesterday = today.AddDays(-1).AddHours(3).AddMinutes(50);
        var twoDaysAgo = today.AddDays(-2).AddHours(6).AddMinutes(40);

        var kim = new Customer
        {
            Name = "김민수",
            PhoneNumber = "010-1234-5678",
            VehicleName = "K5 하이브리드",
            CompanyDbReference = "회사 DB #A-1042",
            Memo = "월 50만원 이하 희망, 배우자와 상의 예정",
            Status = CustomerStatus.Consulting,
            StatusChangedAt = today.AddDays(-1),
            LastContactAttemptAt = today,
            LastConsultedAt = today,
            CreatedAt = today.AddDays(-2),
            UpdatedAt = today,
        };
        kim.ConsultationLogs.Add(new ConsultationLog { Content = "K5 문의. 월 50만원 이하 희망. 배우자와 상의 후 재연락 예정.", CreatedAt = today, UpdatedAt = today });
        kim.ConsultationLogs.Add(new ConsultationLog { Content = "초기비용 낮은 조건 선호. 박지훈 고객의 K5 견적 조건 참고 가능.", CreatedAt = yesterday, UpdatedAt = yesterday });
        kim.Estimates.Add(new Estimate { OriginalFileName = "estimate_20260707_k5.png", StoredFileName = "estimate_20260707_k5.png", FilePath = "storage/customers/1/estimates/estimate_20260707_k5.png", VehicleName = "K5 하이브리드", MonthlyFee = 498000, Status = "안내완료", CreatedAt = today });
        kim.Attachments.Add(new Attachment { OriginalFileName = "license_kim.jpg", StoredFileName = "license_kim.jpg", FilePath = "storage/customers/1/attachments/license_kim.jpg", FileType = "면허증", CreatedAt = today.AddHours(-1) });

        var park = new Customer
        {
            Name = "박지훈",
            PhoneNumber = "010-7788-9012",
            VehicleName = "K5 / 쏘나타 비교",
            CompanyDbReference = "회사 DB #B-2210",
            Memo = "초기비용 낮은 조건 선호",
            Status = CustomerStatus.NoAnswer,
            StatusChangedAt = today.AddDays(-3),
            LastContactAttemptAt = today.AddDays(-3),
            LastConsultedAt = yesterday,
            CreatedAt = today.AddDays(-5),
            UpdatedAt = yesterday,
        };
        park.ConsultationLogs.Add(new ConsultationLog { Content = "부재. K5와 쏘나타 중 초기비용 낮은 조건으로 재안내 예정.", CreatedAt = yesterday, UpdatedAt = yesterday });
        park.Estimates.Add(new Estimate { OriginalFileName = "estimate_20260704_k5_lowdown.jpg", StoredFileName = "estimate_20260704_k5_lowdown.jpg", FilePath = "storage/customers/2/estimates/estimate_20260704_k5_lowdown.jpg", VehicleName = "K5", MonthlyFee = 512000, Status = "초안", CreatedAt = twoDaysAgo });

        var lee = new Customer
        {
            Name = "이서연",
            PhoneNumber = "010-4567-2200",
            VehicleName = "아반떼 / K5 문의",
            CompanyDbReference = "회사 DB #C-0188",
            Memo = "사업자등록증 확인 필요",
            Status = CustomerStatus.Screening,
            StatusChangedAt = twoDaysAgo,
            LastContactAttemptAt = twoDaysAgo,
            LastConsultedAt = twoDaysAgo,
            CreatedAt = today.AddDays(-4),
            UpdatedAt = twoDaysAgo,
        };
        lee.ConsultationLogs.Add(new ConsultationLog { Content = "사업자등록증 확인 후 심사 진행. 아반떼와 K5 조건 비교.", CreatedAt = twoDaysAgo, UpdatedAt = twoDaysAgo });
        lee.Attachments.Add(new Attachment { OriginalFileName = "business_registration.pdf", StoredFileName = "business_registration.pdf", FilePath = "storage/customers/3/attachments/business_registration.pdf", FileType = "사업자등록증", CreatedAt = twoDaysAgo });

        dbContext.Customers.AddRange(kim, park, lee);
        dbContext.SaveChanges();
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

    private static bool Contains(string? source, string keyword)
    {
        return source?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
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
}
