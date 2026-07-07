using ConsultNote.Infrastructure;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using ConsultNote.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ConsultNote;

public partial class MainWindow : Window
{
    private const string MileagePlaceholder = "0.0~5.0";
    private const string PrepaymentPlaceholder = "선납금";
    private const string DepositPlaceholder = "보증금";
    private readonly GridLength _openSidebarWidth = new(360);
    private readonly GridLength _closedSidebarWidth = new(94);
    private bool _isSidebarOpen = true;

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        viewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        DataContext = viewModel;
        UpdateMileageCustomInput();
        LoadSelectedCustomerConditionForm();
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarOpen = !_isSidebarOpen;

        SidebarColumn.Width = _isSidebarOpen ? _openSidebarWidth : _closedSidebarWidth;
        SidebarPanel.Visibility = _isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
        SidebarCollapsedPanel.Visibility = _isSidebarOpen ? Visibility.Collapsed : Visibility.Visible;
        SidebarToggleButton.Content = "‹";
        SidebarCollapsedToggleButton.Content = "›";
        SidebarToggleButton.ToolTip = _isSidebarOpen ? "파일 닫기" : "파일 열기";
        SidebarCollapsedToggleButton.ToolTip = SidebarToggleButton.ToolTip;
    }

    private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = GetViewModel();
        if (viewModel is null)
        {
            return;
        }

        var dialog = new AddCustomerDialog
        {
            Owner = this,
            DataContext = viewModel,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _ = viewModel.AddCustomer(
                dialog.CustomerName,
                dialog.CustomerPhone,
                dialog.CustomerVehicleName);
    }

    private void VehicleManagementButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VehicleManagementDialog
        {
            Owner = this,
        };

        dialog.ShowDialog();
        GetViewModel()?.LoadVehicleOptions();
    }

    private void ConsultationExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetMileageValue(out _))
        {
            return;
        }

        var exportText = BuildConsultationExportText();
        var exportPath = string.Empty;
        try
        {
            Clipboard.SetText(exportText);

            var exportDirectory = Path.Combine(AppPaths.LogsDirectory, "exports");
            Directory.CreateDirectory(exportDirectory);

            var customerName = SanitizeFileName(GetSelectedCustomer()?.Name ?? "consultation");
            exportPath = Path.Combine(exportDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_{customerName}.txt");
            File.WriteAllText(exportPath, exportText, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"첫통화 양식 저장 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(
            $"첫통화 양식을 클립보드에 복사했습니다.\n회사 사이트 상담내역에 바로 붙여넣을 수 있습니다.\n\n보조 txt 저장 위치:\n{exportPath}",
            "Consult Note",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ConditionFormSaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            return;
        }

        if (!TryGetMileageValue(out var mileage))
        {
            return;
        }

        try
        {
            using var dbContext = new AppDbContext();
            var customer = dbContext.Customers.FirstOrDefault(item => item.Id == selectedCustomer.Id);
            if (customer is null)
            {
                return;
            }

            customer.ContractType = RentRadioButton.IsChecked == true ? "렌트" : "리스";
            customer.FormVehicleBrand = GetSelectedText(VehicleBrandComboBox);
            customer.FormVehicleName = GetSelectedText(VehicleNameComboBox);
            customer.FormFuelType = GetSelectedText(FuelTypeComboBox);
            customer.VehicleName = TrimToNull(customer.FormVehicleName);
            customer.FormVehicleDetail = CleanPlaceholder(VehicleDetailTextBox.Text, "상세 옵션");
            customer.ContractPeriod = GetSelectedText(ContractPeriodComboBox);
            customer.Mileage = mileage;
            customer.DeliveryRegion = GetDeliveryRegionText();
            customer.InitialCost = GetInitialCostText();
            customer.OwnerType = GetOwnerType();
            customer.HasBusinessExperienceOverOneYear = BusinessExperienceToggleButton.IsChecked == true;
            customer.ActualDriver = TrimToNull(ActualDriverTextBox.Text);
            customer.HasDriverLicenseOverOneYear = DriverLicenseToggleButton.IsChecked == true;
            customer.InsuranceAge = GetSelectedText(InsuranceAgeComboBox);
            customer.CreditStatus = TrimToNull(CreditStatusTextBox.Text);
            customer.SpecialNote = TrimToNull(SpecialNoteTextBox.Text);
            customer.IsContractHolderSameAsCustomer = ContractHolderSameAsCustomerCheckBox.IsChecked == true;
            customer.ContractHolderName = TrimToNull(ContractHolderNameTextBox.Text);
            customer.ContractHolderPhoneNumber = PhoneNumberFormatter.Normalize(ContractHolderPhoneTextBox.Text);
            customer.UpdatedAt = DateTime.Now;

            dbContext.SaveChanges();
            GetViewModel()?.ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void ConsultationLogSaveButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            return;
        }

        var content = ConsultationContentTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            MessageBox.Show("상담내역을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var now = DateTime.Now;
            var viewModel = GetViewModel();
            using var dbContext = new AppDbContext();
            var customer = dbContext.Customers.FirstOrDefault(item => item.Id == selectedCustomer.Id);
            if (customer is null)
            {
                return;
            }

            dbContext.ConsultationLogs.Add(new ConsultationLog
            {
                CustomerId = customer.Id,
                Content = content,
                CreatedAt = now,
                UpdatedAt = now,
            });

            customer.LastConsultedAt = now;
            customer.LastContactAttemptAt = now;
            var nextStatus = viewModel?.SelectedCustomerStatus?.Status ?? CustomerStatus.Consulting;
            if (customer.Status != nextStatus)
            {
                customer.Status = nextStatus;
                customer.StatusChangedAt = now;
            }

            customer.UpdatedAt = now;

            dbContext.SaveChanges();
            ConsultationContentTextBox.Text = string.Empty;
            viewModel?.ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void SimilarConditionSearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = GetViewModel();
        if (viewModel is null)
        {
            return;
        }

        var vehicleName = GetSelectedText(VehicleNameComboBox);
        var searchText = TrimToNull(vehicleName) ?? TrimToNull(GetSelectedCustomer()?.VehicleName);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            MessageBox.Show("검색할 차량이나 조건을 먼저 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        viewModel.SearchText = searchText;
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCustomer))
        {
            LoadSelectedCustomerConditionForm();
            ConsultationContentTextBox.Text = string.Empty;
            UpdateSelectedFilePreview();
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.SelectedFile))
        {
            UpdateSelectedFilePreview();
        }
    }

    private void AddCustomerFileButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            MessageBox.Show("파일을 추가할 고객을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new AddCustomerFileDialog
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var now = DateTime.Now;
        var customerFilesDirectory = Path.Combine(AppPaths.CustomersDirectory, selectedCustomer.Id.ToString(CultureInfo.InvariantCulture), "files");
        var destinationPath = string.Empty;
        var storedFileName = string.Empty;

        try
        {
            Directory.CreateDirectory(customerFilesDirectory);

            var extension = Path.GetExtension(dialog.SourceFilePath);
            var unique = Guid.NewGuid().ToString("N")[..8];
            storedFileName = $"{now:yyyyMMdd_HHmmss}_{unique}{extension}";
            destinationPath = Path.Combine(customerFilesDirectory, storedFileName);

            File.Copy(dialog.SourceFilePath, destinationPath, overwrite: false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 복사 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using var dbContext = new AppDbContext();
            dbContext.CustomerFiles.Add(new CustomerFile
            {
                CustomerId = selectedCustomer.Id,
                OriginalFileName = dialog.OriginalFileName,
                StoredFileName = storedFileName,
                DisplayName = dialog.DisplayName,
                FilePath = destinationPath,
                FileType = dialog.FileType,
                CustomFileType = dialog.FileType == "기타" ? dialog.CustomFileType : null,
                Memo = dialog.Memo,
                CreatedAt = now,
            });

            dbContext.SaveChanges();
            GetViewModel()?.ReloadCustomers(selectedCustomer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var selectedFile = GetViewModel()?.SelectedFile;
        if (selectedFile is null)
        {
            return;
        }

        if (!File.Exists(selectedFile.FilePath))
        {
            MessageBox.Show($"파일 경로가 존재하지 않습니다.\n\n{selectedFile.FilePath}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(selectedFile.FilePath)
            {
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일을 열 수 없습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MileageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
        {
            return;
        }

        UpdateMileageCustomInput();
    }

    private void MileageCustomTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (MileageCustomTextBox.Text == MileagePlaceholder)
        {
            MileageCustomTextBox.Text = string.Empty;
            MileageCustomTextBox.Foreground = SystemColors.ControlTextBrush;
        }
    }

    private void MileageCustomTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (MileageCustomTextBox.IsEnabled && string.IsNullOrWhiteSpace(MileageCustomTextBox.Text))
        {
            SetMileagePlaceholder();
        }
    }

    private void InitialCostTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var placeholder = GetInitialCostPlaceholder(textBox);
        if (textBox.Text == placeholder)
        {
            textBox.Text = string.Empty;
            textBox.Foreground = SystemColors.ControlTextBrush;
        }
    }

    private void InitialCostTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || !string.IsNullOrWhiteSpace(textBox.Text))
        {
            return;
        }

        SetInitialCostPlaceholder(textBox);
    }

    private void CustomerPhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.Text = PhoneNumberFormatter.Format(textBox.Text);
        }
    }

    private void ContractHolderSameAsCustomerCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        var useCustomerInfo = ContractHolderSameAsCustomerCheckBox.IsChecked == true;
        if (useCustomerInfo)
        {
            ContractHolderNameTextBox.Text = GetSelectedCustomer()?.Name == "-" ? string.Empty : GetSelectedCustomer()?.Name ?? string.Empty;
            ContractHolderPhoneTextBox.Text = PhoneNumberFormatter.Format(GetSelectedCustomer()?.PhoneNumber);
        }
        else if (ContractHolderSameAsCustomerCheckBox.IsFocused)
        {
            ContractHolderNameTextBox.Text = string.Empty;
            ContractHolderPhoneTextBox.Text = string.Empty;
        }

        ContractHolderNameTextBox.IsEnabled = !useCustomerInfo;
        ContractHolderPhoneTextBox.IsEnabled = !useCustomerInfo;
    }

    private string BuildConsultationExportText()
    {
        var vehicleSpec = string.Join(" ", new[]
        {
            GetSelectedText(VehicleBrandComboBox),
            GetSelectedText(VehicleNameComboBox),
            GetSelectedText(FuelTypeComboBox),
            CleanPlaceholder(VehicleDetailTextBox.Text, "상세 옵션"),
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        _ = TryGetMileageValue(out var mileage);

        var consultationContent = GetLatestConsultationContent();
        var specialNote = GetSpecialNoteExportText();

        return string.Join(Environment.NewLine, new[]
        {
            $"★렌트/리스 : {(RentRadioButton.IsChecked == true ? "렌트" : "리스")}",
            $"★차량스팩 : {vehicleSpec}",
            $"★계약기간 : {GetSelectedText(ContractPeriodComboBox)}",
            $"★주행거리 : {mileage}",
            $"★인도지역 : {GetDeliveryRegionText()}",
            $"★초기비용 : {GetInitialCostText()}",
            $"★명의형태 : {GetOwnerType()}",
            $"★사업업력 : {FormatToggle(BusinessExperienceToggleButton.IsChecked)}",
            $"★실운전자 : {ActualDriverTextBox.Text.Trim()}",
            $"★운전면허 : {FormatToggle(DriverLicenseToggleButton.IsChecked)}",
            $"★보험연령 : {GetSelectedText(InsuranceAgeComboBox)}",
            $"★신용상태 : {CreditStatusTextBox.Text.Trim()}",
            $"★특이사항 : {specialNote}",
            $"★상담내용 : {consultationContent}",
        });
    }

    private void UpdateMileageCustomInput()
    {
        var isCustomMileage = GetMileageSelectedText() == "직접입력";
        MileageCustomTextBox.IsEnabled = isCustomMileage;

        if (isCustomMileage)
        {
            if (string.IsNullOrWhiteSpace(MileageCustomTextBox.Text))
            {
                SetMileagePlaceholder();
            }

            return;
        }

        MileageCustomTextBox.Text = string.Empty;
        MileageCustomTextBox.Foreground = SystemColors.ControlTextBrush;
    }

    private void SetMileagePlaceholder()
    {
        MileageCustomTextBox.Text = MileagePlaceholder;
        MileageCustomTextBox.Foreground = SystemColors.GrayTextBrush;
    }

    private CustomerItemViewModel? GetSelectedCustomer()
    {
        return GetViewModel()?.SelectedCustomer;
    }

    private void UpdateSelectedFilePreview()
    {
        var selectedFile = GetViewModel()?.SelectedFile;
        SelectedFilePreviewImage.Source = null;
        SelectedFilePreviewImage.Visibility = Visibility.Collapsed;
        SelectedFilePreviewLabel.Visibility = Visibility.Visible;

        if (selectedFile is null || !selectedFile.IsImage || !File.Exists(selectedFile.FilePath))
        {
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(selectedFile.FilePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            SelectedFilePreviewImage.Source = bitmap;
            SelectedFilePreviewImage.Visibility = Visibility.Visible;
            SelectedFilePreviewLabel.Visibility = Visibility.Collapsed;
        }
        catch
        {
            SelectedFilePreviewImage.Source = null;
            SelectedFilePreviewImage.Visibility = Visibility.Collapsed;
            SelectedFilePreviewLabel.Visibility = Visibility.Visible;
        }
    }

    private MainWindowViewModel? GetViewModel()
    {
        return DataContext as MainWindowViewModel;
    }

    private string GetLatestConsultationContent()
    {
        var currentInput = ConsultationContentTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(currentInput))
        {
            return currentInput;
        }

        return GetSelectedCustomer()?.ConsultationLogs.FirstOrDefault()?.Content ?? string.Empty;
    }

    private void LoadSelectedCustomerConditionForm()
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            ClearConditionForm();
            return;
        }

        using var dbContext = new AppDbContext();
        var customer = dbContext.Customers.AsNoTracking().FirstOrDefault(item => item.Id == selectedCustomer.Id);
        if (customer is null)
        {
            ClearConditionForm();
            return;
        }

        ClearConditionForm();
        RentRadioButton.IsChecked = customer.ContractType != "리스";
        LeaseRadioButton.IsChecked = customer.ContractType == "리스";
        SetComboText(VehicleBrandComboBox, customer.FormVehicleBrand);
        SetComboText(VehicleNameComboBox, customer.FormVehicleName);
        SetComboText(FuelTypeComboBox, customer.FormFuelType);
        VehicleDetailTextBox.Text = string.IsNullOrWhiteSpace(customer.FormVehicleDetail) ? "상세 옵션" : customer.FormVehicleDetail;
        SetComboText(ContractPeriodComboBox, customer.ContractPeriod);
        SetMileageControl(customer.Mileage);
        SetDeliveryRegion(customer.DeliveryRegion);
        SetInitialCost(customer.InitialCost);
        SetOwnerType(customer.OwnerType);
        BusinessExperienceToggleButton.IsChecked = customer.HasBusinessExperienceOverOneYear == true;
        ActualDriverTextBox.Text = customer.ActualDriver ?? string.Empty;
        DriverLicenseToggleButton.IsChecked = customer.HasDriverLicenseOverOneYear == true;
        SetComboText(InsuranceAgeComboBox, customer.InsuranceAge);
        CreditStatusTextBox.Text = customer.CreditStatus ?? string.Empty;
        SpecialNoteTextBox.Text = customer.SpecialNote ?? string.Empty;
        ContractHolderSameAsCustomerCheckBox.IsChecked = customer.IsContractHolderSameAsCustomer;
        ContractHolderNameTextBox.Text = customer.ContractHolderName ?? string.Empty;
        ContractHolderPhoneTextBox.Text = PhoneNumberFormatter.Format(customer.ContractHolderPhoneNumber);
        ContractHolderSameAsCustomerCheckBox_Changed(ContractHolderSameAsCustomerCheckBox, new RoutedEventArgs());
    }

    private void ClearConditionForm()
    {
        RentRadioButton.IsChecked = true;
        LeaseRadioButton.IsChecked = false;
        VehicleBrandComboBox.SelectedItem = null;
        VehicleBrandComboBox.Text = string.Empty;
        VehicleNameComboBox.SelectedItem = null;
        VehicleNameComboBox.Text = string.Empty;
        FuelTypeComboBox.SelectedItem = null;
        FuelTypeComboBox.Text = string.Empty;
        VehicleDetailTextBox.Text = "상세 옵션";
        ContractPeriodComboBox.SelectedIndex = 2;
        SetMileageControl(null);
        DeliveryRegionComboBox.SelectedIndex = 0;
        DeliveryRegionDetailTextBox.Text = string.Empty;
        NoInitialCostCheckBox.IsChecked = false;
        SetInitialCostPlaceholder(PrepaymentTextBox);
        SetInitialCostPlaceholder(DepositTextBox);
        PersonalOwnerRadioButton.IsChecked = true;
        SoleProprietorOwnerRadioButton.IsChecked = false;
        CorporateOwnerRadioButton.IsChecked = false;
        BusinessExperienceToggleButton.IsChecked = false;
        ActualDriverTextBox.Text = string.Empty;
        DriverLicenseToggleButton.IsChecked = false;
        InsuranceAgeComboBox.SelectedIndex = 1;
        CreditStatusTextBox.Text = string.Empty;
        SpecialNoteTextBox.Text = string.Empty;
        ContractHolderSameAsCustomerCheckBox.IsChecked = false;
        ContractHolderNameTextBox.Text = string.Empty;
        ContractHolderPhoneTextBox.Text = string.Empty;
        ContractHolderNameTextBox.IsEnabled = true;
        ContractHolderPhoneTextBox.IsEnabled = true;
    }

    private string GetOwnerType()
    {
        if (SoleProprietorOwnerRadioButton.IsChecked == true)
        {
            return "개인사업자";
        }

        if (CorporateOwnerRadioButton.IsChecked == true)
        {
            return "법인사업자";
        }

        return "개인";
    }

    private static string FormatToggle(bool? isChecked)
    {
        return isChecked == true ? "1년 이상" : "1년 미만 또는 확인 필요";
    }

    private bool TryGetMileageValue(out string mileage)
    {
        mileage = GetMileageSelectedText();
        if (mileage != "직접입력")
        {
            return true;
        }

        var customMileage = CleanPlaceholder(MileageCustomTextBox.Text, MileagePlaceholder);
        if (string.IsNullOrWhiteSpace(customMileage))
        {
            MessageBox.Show("직접입력 주행거리를 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            mileage = string.Empty;
            return false;
        }

        if (!IsValidCustomMileage(customMileage))
        {
            MessageBox.Show("주행거리는 0~5 사이, 소수점 1자리까지 입력할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            mileage = string.Empty;
            return false;
        }

        mileage = $"{customMileage}만km";
        return true;
    }

    private string? GetMileageSearchTerm()
    {
        var mileage = GetMileageSelectedText();
        if (mileage != "직접입력")
        {
            return mileage;
        }

        var customMileage = CleanPlaceholder(MileageCustomTextBox.Text, MileagePlaceholder);
        return string.IsNullOrWhiteSpace(customMileage) ? null : $"{customMileage}만km";
    }

    private string GetContractHolderText()
    {
        var name = ContractHolderNameTextBox.Text.Trim();
        var phoneNumber = PhoneNumberFormatter.Format(ContractHolderPhoneTextBox.Text);
        var values = new[] { name, phoneNumber }.Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join(" / ", values);
    }

    private string GetSpecialNoteExportText()
    {
        var specialNote = SpecialNoteTextBox.Text.Trim();
        var contractHolderText = GetContractHolderText();
        if (string.IsNullOrWhiteSpace(contractHolderText))
        {
            return specialNote;
        }

        return string.IsNullOrWhiteSpace(specialNote)
            ? $"명의자: {contractHolderText}"
            : $"{specialNote} / 명의자: {contractHolderText}";
    }

    private string GetDeliveryRegionText()
    {
        return string.Join(" ", new[]
        {
            GetSelectedText(DeliveryRegionComboBox),
            DeliveryRegionDetailTextBox.Text.Trim(),
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private string GetInitialCostText()
    {
        return NoInitialCostCheckBox.IsChecked == true
            ? "없음"
            : string.Join(" / ", new[]
            {
                CleanPlaceholder(PrepaymentTextBox.Text, "선납금"),
                CleanPlaceholder(DepositTextBox.Text, "보증금"),
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private string GetMileageSelectedText()
    {
        return MileageComboBox.SelectedItem switch
        {
            ComboBoxItem item => item.Content?.ToString() ?? string.Empty,
            string text => text,
            object value => value.ToString() ?? string.Empty,
            _ => MileageComboBox.Text.Trim(),
        };
    }

    private static string GetSelectedText(ComboBox comboBox)
    {
        if (!string.IsNullOrWhiteSpace(comboBox.Text))
        {
            return comboBox.Text.Trim();
        }

        return comboBox.SelectedItem switch
        {
            ComboBoxItem item => item.Content?.ToString() ?? string.Empty,
            string text => text,
            object value => value.ToString() ?? string.Empty,
            _ => string.Empty,
        };
    }

    private static string CleanPlaceholder(string? value, string placeholder)
    {
        var text = value?.Trim() ?? string.Empty;
        return text == placeholder ? string.Empty : text;
    }

    private static bool IsValidCustomMileage(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mileage))
        {
            return false;
        }

        var decimalPartLength = value.Contains('.')
            ? value[(value.IndexOf('.') + 1)..].Length
            : 0;

        return mileage >= 0 && mileage <= 5 && decimalPartLength <= 1;
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static void SetComboText(ComboBox comboBox, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        foreach (var item in comboBox.Items)
        {
            var itemText = item is ComboBoxItem comboBoxItem ? comboBoxItem.Content?.ToString() : item?.ToString();
            if (itemText == text)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.Text = text;
    }

    private void SetMileageControl(string? mileage)
    {
        if (string.IsNullOrWhiteSpace(mileage))
        {
            MileageComboBox.SelectedIndex = 2;
            UpdateMileageCustomInput();
            return;
        }

        if (mileage.EndsWith("만km", StringComparison.Ordinal) &&
            !MileageComboBox.Items.OfType<ComboBoxItem>().Any(item => item.Content?.ToString() == mileage))
        {
            MileageComboBox.SelectedItem = MileageComboBox.Items.OfType<ComboBoxItem>().First(item => item.Content?.ToString() == "직접입력");
            MileageCustomTextBox.Text = mileage.Replace("만km", string.Empty, StringComparison.Ordinal);
            MileageCustomTextBox.Foreground = SystemColors.ControlTextBrush;
            return;
        }

        SetComboText(MileageComboBox, mileage);
        UpdateMileageCustomInput();
    }

    private void SetDeliveryRegion(string? deliveryRegion)
    {
        if (string.IsNullOrWhiteSpace(deliveryRegion))
        {
            return;
        }

        var parts = deliveryRegion.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        SetComboText(DeliveryRegionComboBox, parts[0]);
        DeliveryRegionDetailTextBox.Text = parts.Length > 1 ? parts[1] : string.Empty;
    }

    private void SetInitialCost(string? initialCost)
    {
        if (string.IsNullOrWhiteSpace(initialCost))
        {
            NoInitialCostCheckBox.IsChecked = false;
            SetInitialCostPlaceholder(PrepaymentTextBox);
            SetInitialCostPlaceholder(DepositTextBox);
            return;
        }

        NoInitialCostCheckBox.IsChecked = initialCost == "없음";
        if (NoInitialCostCheckBox.IsChecked == true)
        {
            SetInitialCostPlaceholder(PrepaymentTextBox);
            SetInitialCostPlaceholder(DepositTextBox);
            return;
        }

        var parts = initialCost.Split(" / ", StringSplitOptions.TrimEntries);
        SetInitialCostTextOrPlaceholder(PrepaymentTextBox, parts.ElementAtOrDefault(0));
        SetInitialCostTextOrPlaceholder(DepositTextBox, parts.ElementAtOrDefault(1));
    }

    private static string GetInitialCostPlaceholder(TextBox textBox)
    {
        return textBox.Name == nameof(DepositTextBox) ? DepositPlaceholder : PrepaymentPlaceholder;
    }

    private static void SetInitialCostTextOrPlaceholder(TextBox textBox, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetInitialCostPlaceholder(textBox);
            return;
        }

        textBox.Text = value;
        textBox.Foreground = SystemColors.ControlTextBrush;
    }

    private static void SetInitialCostPlaceholder(TextBox textBox)
    {
        textBox.Text = GetInitialCostPlaceholder(textBox);
        textBox.Foreground = SystemColors.GrayTextBrush;
    }

    private void SetOwnerType(string? ownerType)
    {
        PersonalOwnerRadioButton.IsChecked = string.IsNullOrWhiteSpace(ownerType) || ownerType == "개인";
        SoleProprietorOwnerRadioButton.IsChecked = ownerType == "개인사업자";
        CorporateOwnerRadioButton.IsChecked = ownerType == "법인사업자";
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(fileName) ? "consultation" : fileName;
    }

    private static void ShowDatabaseSaveError(Exception exception)
    {
        MessageBox.Show(
            $"저장 중 오류가 발생했습니다.\n\n{exception.Message}",
            "Consult Note",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
