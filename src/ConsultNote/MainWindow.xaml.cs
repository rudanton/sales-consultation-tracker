using ConsultNote.Infrastructure;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using ConsultNote.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ConsultNote;

public partial class MainWindow : Window
{
    private const string AllFileTypesFilter = "전체";
    private const string MileagePlaceholder = "0.0~5.0";
    private const string VehicleDetailPlaceholder = "상세 옵션";
    private const string DeliveryRegionDetailPlaceholder = "상세 지역";
    private readonly GridLength _openSidebarWidth = new(360);
    private readonly GridLength _closedSidebarWidth = new(94);
    private int? _currentSelectedCustomerId;
    private int? _editingConsultationLogId;
    private bool _isSidebarOpen;

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        viewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        DataContext = viewModel;
        ApplySidebarState();
        ApplyAppVersion();
        EnsureFileListOptions();
        UpdateMileageCustomInput();
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void ApplyAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersionTextBlock.Text = version is null
            ? string.Empty
            : $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    private static Version GetCurrentAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null
            ? new Version(0, 0, 0)
            : new Version(version.Major, version.Minor, version.Build);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        WindowPlacementStore.Apply(this);
        _currentSelectedCustomerId = GetSelectedCustomer()?.Id;
        Dispatcher.BeginInvoke(() => RefreshSelectedCustomerUi(scrollConditionToTop: true), DispatcherPriority.ContextIdle);

        Show();
        Activate();
        Focus();

        Topmost = true;
        Topmost = false;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        WindowPlacementStore.Save(this);
    }

    private void RefreshSelectedCustomerUi(bool scrollConditionToTop = false)
    {
        LoadSelectedCustomerConditionForm();
        if (scrollConditionToTop)
        {
            ConditionFormScrollViewer.ScrollToTop();
        }

        RefreshFileListControls();
        RefreshReferenceEstimateList();
        UpdateSelectedFilePreview();
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarOpen = !_isSidebarOpen;
        ApplySidebarState();
    }

    private void ApplySidebarState()
    {
        SidebarColumn.Width = _isSidebarOpen ? _openSidebarWidth : _closedSidebarWidth;
        SidebarPanel.Visibility = _isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
        SidebarCollapsedPanel.Visibility = _isSidebarOpen ? Visibility.Collapsed : Visibility.Visible;
        SidebarToggleButton.Content = "사이드바 접기";
        SidebarCollapsedToggleButton.Content = "파일 ›";
        SidebarToggleButton.ToolTip = _isSidebarOpen ? "파일 사이드바 접기" : "파일 열기";
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
                vehicleName: null);
    }

    private void VehicleManagementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new VehicleManagementDialog
            {
                Owner = this,
            };

            dialog.ShowDialog();
            GetViewModel()?.LoadVehicleOptions();
        }
        catch (Exception ex)
        {
            LogUiError(ex, "vehicle-management-error.txt");
            MessageBox.Show(
                $"차종 목록 화면을 열 수 없습니다.\n\n{ex.Message}\n\nlogs 폴더의 vehicle-management-error.txt를 확인해주세요.",
                "Consult Note",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void VehicleResourceManagementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new VehicleResourceManagementDialog
            {
                Owner = this,
            };

            dialog.ShowDialog();
            RefreshReferenceEstimateList();
        }
        catch (Exception ex)
        {
            LogUiError(ex, "vehicle-resource-management-error.txt");
            MessageBox.Show(
                $"차량별 자료 화면을 열 수 없습니다.\n\n{ex.Message}\n\nlogs 폴더의 vehicle-resource-management-error.txt를 확인해주세요.",
                "Consult Note",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentVersion = GetCurrentAppVersion();
            var result = await new GitHubReleaseUpdateChecker().CheckLatestRelease(currentVersion);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!result.HasUpdate)
            {
                MessageBox.Show($"현재 최신 버전입니다.\n\n현재 버전: v{currentVersion}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openRelease = MessageBox.Show(
                $"새 버전이 있습니다.\n\n현재 버전: v{currentVersion}\n최신 버전: v{result.LatestVersion}\n\nGitHub Release 페이지를 열까요?",
                "Consult Note",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            if (openRelease == MessageBoxResult.Yes && !string.IsNullOrWhiteSpace(result.ReleaseUrl))
            {
                Process.Start(new ProcessStartInfo(result.ReleaseUrl)
                {
                    UseShellExecute = true,
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"업데이트 확인 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CopyCustomerPhoneButton_Click(object sender, RoutedEventArgs e)
    {
        var phoneNumber = PhoneNumberFormatter.Format(GetSelectedCustomer()?.PhoneNumber ?? CustomerPhoneTextBox.Text);
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber == "-")
        {
            MessageBox.Show("복사할 연락처가 없습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Clipboard.SetText(phoneNumber);
    }

    private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = GetViewModel();
        if (viewModel is not null)
        {
            viewModel.SearchText = string.Empty;
        }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
        {
            e.Handled = true;
            AddCustomerButton_Click(this, new RoutedEventArgs());
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            e.Handled = true;
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            return;
        }

        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        if (ConsultationContentTextBox.IsKeyboardFocusWithin)
        {
            e.Handled = true;
            ConsultationLogSaveButton_Click(ConsultationLogSaveButton, new RoutedEventArgs());
            return;
        }

        if (BasicInfoPanel.IsKeyboardFocusWithin || ConditionFormPanel.IsKeyboardFocusWithin)
        {
            e.Handled = true;
            ConditionFormSaveButton_Click(this, new RoutedEventArgs());
        }
    }

    private void ConsultationContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || Keyboard.Modifiers != ModifierKeys.None)
        {
            return;
        }

        e.Handled = true;
        ConsultationLogSaveButton.Focus();
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

    private void ConsultationHistoryExportButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            MessageBox.Show("상담 기록을 내보낼 고객을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (selectedCustomer.ConsultationLogs.Count == 0)
        {
            MessageBox.Show("내보낼 상담 기록이 없습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var exportPath = string.Empty;
        try
        {
            var exportDirectory = GetCustomerFilesDirectory(selectedCustomer.Id);
            Directory.CreateDirectory(exportDirectory);

            var customerName = SanitizeFileName(selectedCustomer.Name);
            exportPath = Path.Combine(exportDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}_{customerName}_상담기록.txt");
            File.WriteAllText(exportPath, BuildConsultationHistoryExportText(selectedCustomer), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"상담 기록 내보내기 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(
            $"고객 1명의 전체 상담 기록을 txt 파일 1개로 저장했습니다.\n과거 기록부터 최신 기록 순서로 정리됩니다.\n\n{exportPath}",
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

            if (!TrySaveBasicInfoToCustomer(customer))
            {
                return;
            }

            SaveConditionFormToCustomer(customer, mileage);
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

        if (!TryGetMileageValue(out var mileage))
        {
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

            if (!TrySaveBasicInfoToCustomer(customer))
            {
                return;
            }

            SaveConditionFormToCustomer(customer, mileage);

            if (_editingConsultationLogId is not null)
            {
                var log = dbContext.ConsultationLogs.FirstOrDefault(item =>
                    item.Id == _editingConsultationLogId.Value &&
                    item.CustomerId == customer.Id);
                if (log is null)
                {
                    MessageBox.Show("수정할 상담 기록을 찾을 수 없습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ClearConsultationLogEditMode();
                    return;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    MessageBox.Show("수정할 상담 내용을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                log.Content = content;
                log.UpdatedAt = now;
            }
            else if (!string.IsNullOrWhiteSpace(content))
            {
                dbContext.ConsultationLogs.Add(new ConsultationLog
                {
                    CustomerId = customer.Id,
                    Content = content,
                    CreatedAt = now,
                    UpdatedAt = now,
                });

                customer.LastConsultedAt = now;
            }

            customer.LastContactAttemptAt = now;
            var nextStatus = viewModel?.SelectedCustomerStatus?.Status ?? CustomerStatus.Consulting;
            if (customer.Status != nextStatus)
            {
                customer.Status = nextStatus;
                customer.StatusChangedAt = now;
            }

            customer.UpdatedAt = now;

            dbContext.SaveChanges();
            ClearConsultationLogEditMode();
            viewModel?.ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void EditConsultationLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int logId })
        {
            return;
        }

        var selectedCustomer = GetSelectedCustomer();
        var log = selectedCustomer?.ConsultationLogs.FirstOrDefault(item => item.Id == logId);
        if (log is null)
        {
            return;
        }

        _editingConsultationLogId = logId;
        ConsultationContentTextBox.Text = log.Content;
        ConsultationContentTextBox.Focus();
        ConsultationContentTextBox.CaretIndex = ConsultationContentTextBox.Text.Length;
        ConsultationLogSaveButton.Content = "수정 저장";
    }

    private void DeleteConsultationLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int logId })
        {
            return;
        }

        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            return;
        }

        var result = MessageBox.Show(
            "상담 기록을 삭제할까요?",
            "Consult Note",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var dbContext = new AppDbContext();
            var log = dbContext.ConsultationLogs.FirstOrDefault(item =>
                item.Id == logId &&
                item.CustomerId == selectedCustomer.Id);
            if (log is null)
            {
                return;
            }

            dbContext.ConsultationLogs.Remove(log);

            var customer = dbContext.Customers.FirstOrDefault(item => item.Id == selectedCustomer.Id);
            if (customer is not null)
            {
                customer.LastConsultedAt = dbContext.ConsultationLogs
                    .Where(item => item.CustomerId == selectedCustomer.Id && item.Id != logId)
                    .OrderByDescending(item => item.CreatedAt)
                    .Select(item => (DateTime?)item.CreatedAt)
                    .FirstOrDefault();
                customer.UpdatedAt = DateTime.Now;
            }

            dbContext.SaveChanges();
            if (_editingConsultationLogId == logId)
            {
                ClearConsultationLogEditMode();
            }

            GetViewModel()?.ReloadCustomers(selectedCustomer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void ClearConsultationLogEditMode()
    {
        _editingConsultationLogId = null;
        ConsultationContentTextBox.Text = string.Empty;
        ConsultationLogSaveButton.Content = "저장";
    }

    private void SimilarConditionSearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = GetViewModel();
        if (viewModel is null)
        {
            return;
        }

        var searchText = GetSelectedVehicleNameForSearch() ?? TrimToNull(GetSelectedCustomer()?.VehicleName);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            MessageBox.Show("차량별 자료를 찾을 차량명을 먼저 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        viewModel.SearchText = searchText;
    }

    private void FavoriteToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
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

            customer.IsFavorite = FavoriteToggleButton.IsChecked == true;
            customer.UpdatedAt = DateTime.Now;
            dbContext.SaveChanges();
            GetViewModel()?.ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void DiscardCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            return;
        }

        var dialog = new DiscardCustomerDialog
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var now = DateTime.Now;
            using var dbContext = new AppDbContext();
            var customer = dbContext.Customers.FirstOrDefault(item => item.Id == selectedCustomer.Id);
            if (customer is null)
            {
                return;
            }

            customer.Status = CustomerStatus.Discarded;
            customer.StatusChangedAt = now;
            customer.LastContactAttemptAt = now;
            customer.DiscardReason = dialog.Reason;
            customer.UpdatedAt = now;
            dbContext.SaveChanges();
            GetViewModel()?.ReloadCustomers(customer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void RestoreCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            return;
        }

        try
        {
            var now = DateTime.Now;
            using var dbContext = new AppDbContext();
            var customer = dbContext.Customers.FirstOrDefault(item => item.Id == selectedCustomer.Id);
            if (customer is null)
            {
                return;
            }

            customer.Status = CustomerStatus.Consulting;
            customer.StatusChangedAt = now;
            customer.DiscardReason = null;
            customer.UpdatedAt = now;
            dbContext.SaveChanges();

            var viewModel = GetViewModel();
            if (viewModel is not null)
            {
                viewModel.ShowDiscardedCustomers = false;
                viewModel.ReloadCustomers(customer.Id);
            }
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCustomer))
        {
            var selectedCustomerId = GetSelectedCustomer()?.Id;
            var isDifferentCustomer = selectedCustomerId != _currentSelectedCustomerId;
            _currentSelectedCustomerId = selectedCustomerId;

            RefreshSelectedCustomerUi(scrollConditionToTop: isDifferentCustomer);
            if (isDifferentCustomer)
            {
                ClearConsultationLogEditMode();
            }
        }
        else if (e.PropertyName == nameof(MainWindowViewModel.SelectedFile))
        {
            UpdateSelectedFilePreview();
        }
    }

    private void AddCustomerFileButton_Click(object sender, RoutedEventArgs e)
    {
        AddCustomerFile(sourceFilePath: null);
    }

    private void CustomerFileDropArea_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = CanAcceptDroppedFiles(e) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void CustomerFileDropArea_Drop(object sender, DragEventArgs e)
    {
        if (!CanAcceptDroppedFiles(e))
        {
            e.Handled = true;
            return;
        }

        var filePaths = ((string[])e.Data.GetData(DataFormats.FileDrop))
            .Where(File.Exists)
            .Where(AddCustomerFileDialog.IsSupportedFile)
            .ToList();

        foreach (var filePath in filePaths)
        {
            if (!AddCustomerFile(filePath))
            {
                break;
            }
        }

        e.Handled = true;
    }

    private bool CanAcceptDroppedFiles(DragEventArgs e)
    {
        if (GetSelectedCustomer() is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        var filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
        return filePaths?.Any(filePath => File.Exists(filePath) && AddCustomerFileDialog.IsSupportedFile(filePath)) == true;
    }

    private bool AddCustomerFile(string? sourceFilePath)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            MessageBox.Show("파일을 추가할 고객을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (sourceFilePath is not null && !AddCustomerFileDialog.IsSupportedFile(sourceFilePath))
        {
            MessageBox.Show(".jpg, .jpeg, .png, .pdf 파일만 추가할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        var dialog = new AddCustomerFileDialog(
            GetOwnerType(),
            GetNextFileOrdersByType(selectedCustomer),
            vehicleBrand: GetSelectedText(VehicleBrandComboBox),
            vehicleName: TrimToNull(GetSelectedText(VehicleNameComboBox)) ?? TrimToNull(selectedCustomer.VehicleName),
            fuelType: GetSelectedText(FuelTypeComboBox),
            sourceFilePath: sourceFilePath)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        var now = DateTime.Now;
        var customerFilesDirectory = GetCustomerFilesDirectory(selectedCustomer.Id);
        var destinationPath = string.Empty;
        var storedFileName = string.Empty;
        using var dbContext = new AppDbContext();
        var fileOrder = GetNextFileOrderFromDatabase(dbContext, selectedCustomer.Id, dialog.FileType, dialog.CustomFileType);
        var displayName = BuildCustomerFileDisplayName(selectedCustomer.Name, dialog.FileType, dialog.CustomFileType, fileOrder);
        var storedBaseName = BuildCustomerStoredFileBaseName(
            selectedCustomer.Name,
            dialog.FileType,
            dialog.CustomFileType,
            fileOrder,
            dialog.Memo);

        try
        {
            Directory.CreateDirectory(customerFilesDirectory);

            var extension = Path.GetExtension(dialog.SourceFilePath);
            storedFileName = BuildStoredCustomerFileName(customerFilesDirectory, storedBaseName, extension);
            destinationPath = Path.Combine(customerFilesDirectory, storedFileName);

            File.Copy(dialog.SourceFilePath, destinationPath, overwrite: false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 복사 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        try
        {
            var customerFile = new CustomerFile
            {
                CustomerId = selectedCustomer.Id,
                OriginalFileName = dialog.OriginalFileName,
                StoredFileName = storedFileName,
                DisplayName = displayName,
                FilePath = destinationPath,
                FileType = dialog.FileType,
                CustomFileType = dialog.FileType == "기타" ? dialog.CustomFileType : null,
                FileOrder = fileOrder,
                Memo = dialog.Memo,
                CreatedAt = now,
            };

            dbContext.CustomerFiles.Add(customerFile);
            LinkEstimateFileToVehicleResource(
                dbContext,
                selectedCustomer.Id,
                selectedCustomer.Name,
                customerFile,
                dialog.VehicleBrand,
                dialog.VehicleName,
                dialog.FuelType,
                capitalCompany: null,
                dialog.RentalCompany,
                now);

            dbContext.SaveChanges();
            GetViewModel()?.ReloadCustomers(selectedCustomer.Id);
            return true;
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
            return false;
        }
    }

    private void DeleteCustomerFileButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = GetViewModel();
        var selectedCustomer = viewModel?.SelectedCustomer;
        var selectedFile = viewModel?.SelectedFile;
        if (viewModel is null || selectedCustomer is null || selectedFile is null)
        {
            MessageBox.Show("삭제할 파일을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"선택한 파일을 삭제할까요?\n\n{selectedFile.FileName}",
            "Consult Note",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var dbContext = new AppDbContext();
            var customerFile = dbContext.CustomerFiles.FirstOrDefault(file =>
                file.Id == selectedFile.Id &&
                file.CustomerId == selectedCustomer.Id);
            if (customerFile is null)
            {
                return;
            }

            var filePath = customerFile.FilePath;
            RemoveVehicleResourcesLinkedToCustomerFile(dbContext, customerFile);
            dbContext.CustomerFiles.Remove(customerFile);
            dbContext.SaveChanges();

            TryDeleteStoredCustomerFile(filePath);
            viewModel.ReloadCustomers(selectedCustomer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
        }
    }

    private void OpenCustomerFilesFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer is null)
        {
            MessageBox.Show("파일 폴더를 열 고객을 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var customerFilesDirectory = GetCustomerFilesDirectory(selectedCustomer.Id);
            Directory.CreateDirectory(customerFilesDirectory);

            Process.Start(new ProcessStartInfo
            {
                FileName = customerFilesDirectory,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 폴더를 열 수 없습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FileListOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFileListView();
        SelectFirstVisibleFileIfNeeded();
    }

    private void VehicleNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        RefreshReferenceEstimateList();
    }

    private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        EditSelectedFileMetadata();
    }

    private void SelectedFilePreview_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount < 2)
        {
            return;
        }

        var selectedFile = GetViewModel()?.SelectedFile;
        if (selectedFile is null)
        {
            return;
        }

        OpenFile(selectedFile);
    }

    private void ReferenceEstimateListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ReferenceEstimateListBox.SelectedItem is FileItemViewModel selectedFile)
        {
            OpenFile(selectedFile);
        }
    }

    private void OpenFile(FileItemViewModel selectedFile)
    {
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

    private void EditSelectedFileMetadata()
    {
        var viewModel = GetViewModel();
        var selectedCustomer = GetSelectedCustomer();
        var selectedFile = viewModel?.SelectedFile;
        if (selectedCustomer is null || selectedFile is null)
        {
            return;
        }

        using var dbContext = new AppDbContext();
        var customerFile = dbContext.CustomerFiles.FirstOrDefault(file => file.Id == selectedFile.Id);
        if (customerFile is null)
        {
            return;
        }

        var linkedVehicleResource = dbContext.CustomerVehicleResourceLinks
            .Include(link => link.VehicleResourceFile)
            .AsNoTracking()
            .Where(link => link.CustomerFileId == customerFile.Id)
            .Select(link => link.VehicleResourceFile)
            .FirstOrDefault();

        var dialog = new AddCustomerFileDialog(
            GetOwnerType(),
            GetNextFileOrdersByType(selectedCustomer),
            customerFile.FileType,
            customerFile.CustomFileType,
            customerFile.FileOrder,
            customerFile.Memo,
            linkedVehicleResource?.VehicleBrand ?? GetSelectedText(VehicleBrandComboBox),
            linkedVehicleResource?.VehicleName ?? GetSelectedText(VehicleNameComboBox),
            linkedVehicleResource?.FuelType ?? GetSelectedText(FuelTypeComboBox),
            capitalCompany: null,
            linkedVehicleResource?.RentalCompany ?? linkedVehicleResource?.CapitalCompany,
            isEditMode: true)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var originalDisplayFileType = GetDisplayFileType(customerFile.FileType, customerFile.CustomFileType);
        var editedDisplayFileType = GetDisplayFileType(dialog.FileType, dialog.CustomFileType);
        var fileOrder = string.Equals(originalDisplayFileType, editedDisplayFileType, StringComparison.CurrentCulture)
            ? customerFile.FileOrder
            : GetNextFileOrderFromDatabase(dbContext, selectedCustomer.Id, dialog.FileType, dialog.CustomFileType, customerFile.Id);

        customerFile.FileType = dialog.FileType;
        customerFile.CustomFileType = dialog.FileType == "기타" ? dialog.CustomFileType : null;
        customerFile.FileOrder = fileOrder;
        customerFile.Memo = dialog.Memo;
        customerFile.DisplayName = BuildCustomerFileDisplayName(
            selectedCustomer.Name,
            dialog.FileType,
            dialog.CustomFileType,
            fileOrder);
        SyncEstimateVehicleResourceLink(
            dbContext,
            selectedCustomer.Id,
            selectedCustomer.Name,
            customerFile,
            dialog.VehicleBrand,
            dialog.VehicleName,
            dialog.FuelType,
            capitalCompany: null,
            dialog.RentalCompany,
            DateTime.Now);

        try
        {
            dbContext.SaveChanges();
            viewModel?.ReloadCustomers(selectedCustomer.Id);
        }
        catch (Exception ex)
        {
            ShowDatabaseSaveError(ex);
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

    private void SaveConditionFormToCustomer(Customer customer, string mileage)
    {
        customer.ContractType = GetContractType();
        customer.FormVehicleBrand = TrimToNull(GetSelectedText(VehicleBrandComboBox));
        customer.FormVehicleName = TrimToNull(GetSelectedText(VehicleNameComboBox));
        customer.FormFuelType = TrimToNull(GetSelectedText(FuelTypeComboBox));
        customer.VehicleName = TrimToNull(customer.FormVehicleName);
        customer.FormVehicleDetail = CleanPlaceholder(VehicleDetailTextBox.Text, VehicleDetailPlaceholder);
        customer.ContractPeriod = TrimToNull(GetSelectedText(ContractPeriodComboBox));
        customer.Mileage = TrimToNull(mileage);
        customer.DeliveryRegion = TrimToNull(GetDeliveryRegionText());
        customer.InitialCost = TrimToNull(GetInitialCostText());
        customer.OwnerType = TrimToNull(GetOwnerType());
        customer.HasBusinessExperienceOverOneYear = BusinessExperienceToggleButton.IsChecked == true;
        customer.ActualDriver = TrimToNull(ActualDriverTextBox.Text);
        customer.HasDriverLicenseOverOneYear = DriverLicenseToggleButton.IsChecked == true;
        customer.InsuranceAge = TrimToNull(GetSelectedText(InsuranceAgeComboBox));
        customer.CreditStatus = TrimToNull(CreditStatusTextBox.Text);
        customer.SpecialNote = TrimToNull(SpecialNoteTextBox.Text);

        var shouldSaveContractHolder = ShouldShowContractHolder();
        customer.IsContractHolderSameAsCustomer = shouldSaveContractHolder && ContractHolderSameAsCustomerCheckBox.IsChecked == true;
        customer.ContractHolderName = shouldSaveContractHolder ? TrimToNull(ContractHolderNameTextBox.Text) : null;
        customer.ContractHolderPhoneNumber = shouldSaveContractHolder ? PhoneNumberFormatter.Normalize(ContractHolderPhoneTextBox.Text) : null;
    }

    private bool TrySaveBasicInfoToCustomer(Customer customer)
    {
        var normalizedName = TrimToNull(GetViewModel()?.CustomerNameInput);
        if (normalizedName is null)
        {
            MessageBox.Show("고객 이름을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        customer.Name = normalizedName;
        customer.PhoneNumber = PhoneNumberFormatter.Normalize(GetViewModel()?.CustomerPhoneInput);
        customer.Memo = TrimToNull(GetViewModel()?.CustomerMemoInput);
        return true;
    }

    private void OwnerTypeRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        UpdateContractHolderVisibility();
    }

    private void UpdateContractHolderVisibility()
    {
        if (!IsInitialized)
        {
            return;
        }

        var shouldShow = ShouldShowContractHolder();
        ContractHolderPanel.Visibility = shouldShow ? Visibility.Visible : Visibility.Collapsed;
        if (!shouldShow)
        {
            ContractHolderSameAsCustomerCheckBox.IsChecked = false;
            ContractHolderNameTextBox.Text = string.Empty;
            ContractHolderPhoneTextBox.Text = string.Empty;
        }
    }

    private bool ShouldShowContractHolder()
    {
        return PersonalOwnerRadioButton.IsChecked == true || SoleProprietorOwnerRadioButton.IsChecked == true;
    }

    private void PlaceholderTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var placeholder = GetPlaceholder(textBox);
        if (textBox.Text == placeholder)
        {
            textBox.Text = string.Empty;
            textBox.Foreground = SystemColors.ControlTextBrush;
        }
    }

    private void PlaceholderTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || !string.IsNullOrWhiteSpace(textBox.Text))
        {
            return;
        }

        SetPlaceholder(textBox);
    }

    private string BuildConsultationExportText()
    {
        var vehicleSpec = string.Join(" ", new[]
        {
            GetSelectedText(VehicleBrandComboBox),
            GetSelectedText(VehicleNameComboBox),
            GetSelectedText(FuelTypeComboBox),
            CleanPlaceholder(VehicleDetailTextBox.Text, VehicleDetailPlaceholder),
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        _ = TryGetMileageValue(out var mileage);

        var consultationContent = GetLatestConsultationContent();
        var specialNote = GetSpecialNoteExportText();

        return string.Join(Environment.NewLine, new[]
        {
            $"★렌트/리스 : {GetContractType()}",
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

    private static string BuildConsultationHistoryExportText(CustomerItemViewModel customer)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"고객명: {customer.Name}");
        builder.AppendLine($"연락처: {customer.PhoneNumber}");
        builder.AppendLine($"차량: {customer.VehicleSummary}");
        builder.AppendLine($"상태: {customer.StatusText}");
        builder.AppendLine($"내보낸 시간: {DateTime.Now:yyyy-MM-dd HH:mm}");
        builder.AppendLine();
        builder.AppendLine("상담 기록 (과거 → 최신)");
        builder.AppendLine("========================================");

        foreach (var log in customer.ConsultationLogs.OrderBy(log => log.CreatedAtText))
        {
            builder.AppendLine();
            builder.AppendLine("----------------------------------------");
            builder.AppendLine($"[{log.CreatedAtText}]");
            builder.AppendLine(log.Content);
        }

        return builder.ToString();
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

    private void EnsureFileListOptions()
    {
        FileTypeFilterComboBox.Items.Clear();
        FileTypeFilterComboBox.Items.Add(AllFileTypesFilter);
        FileTypeFilterComboBox.SelectedItem = AllFileTypesFilter;

        FileSortComboBox.Items.Clear();
        FileSortComboBox.Items.Add("유형/순서");
        FileSortComboBox.Items.Add("최신순");
        FileSortComboBox.Items.Add("파일명순");
        FileSortComboBox.SelectedIndex = 0;
    }

    private void RefreshFileListControls()
    {
        RefreshFileTypeFilterOptions();
        ApplyFileListView();
        SelectFirstVisibleFileIfNeeded();
    }

    private void RefreshReferenceEstimateList()
    {
        var selectedCustomer = GetSelectedCustomer();
        var vehicleName = GetSelectedVehicleNameForSearch() ?? TrimToNull(selectedCustomer?.VehicleName);
        if (selectedCustomer is null || string.IsNullOrWhiteSpace(vehicleName) || vehicleName == "-")
        {
            ReferenceEstimateTitleTextBlock.Text = "차량별 자료";
            ReferenceEstimateListBox.ItemsSource = Array.Empty<FileItemViewModel>();
            return;
        }

        using var dbContext = new AppDbContext();
        var linkedCustomerFileIds = dbContext.CustomerVehicleResourceLinks
            .AsNoTracking()
            .Where(link => link.CustomerFileId != null)
            .Select(link => link.CustomerFileId!.Value)
            .ToHashSet();

        var customerEstimates = dbContext.Customers
            .Include(customer => customer.CustomerFiles)
            .AsNoTracking()
            .Where(customer => customer.Id != selectedCustomer.Id && customer.Status != CustomerStatus.Discarded)
            .AsEnumerable()
            .Where(customer => string.Equals(
                TrimToNull(customer.FormVehicleName) ?? TrimToNull(customer.VehicleName),
                vehicleName,
                StringComparison.CurrentCultureIgnoreCase))
            .SelectMany(customer => customer.CustomerFiles
                .Where(file => IsEstimateFileType(file.FileType, file.CustomFileType))
                .Where(file => !linkedCustomerFileIds.Contains(file.Id))
                .OrderByDescending(file => file.CreatedAt)
                .Select(file => new FileItemViewModel
                {
                    Id = file.Id,
                    FileName = file.DisplayName,
                    FilePath = file.FilePath,
                    FileType = GetDisplayFileType(file.FileType, file.CustomFileType),
                    FileOrder = file.FileOrder,
                    Summary = $"{customer.Name} · {file.CreatedAt:yyyy-MM-dd}",
                    PreviewTitle = file.DisplayName,
                    PreviewMeta = $"{customer.Name} · {file.CreatedAt:yyyy-MM-dd}",
                    PreviewLabel = file.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : GetDisplayFileType(file.FileType, file.CustomFileType),
                    CreatedAt = file.CreatedAt,
                }));

        var vehicleResources = dbContext.VehicleResourceFiles
            .Include(file => file.CustomerLinks)
                .ThenInclude(link => link.Customer)
            .AsNoTracking()
            .AsEnumerable()
            .Where(file => string.Equals(TrimToNull(file.VehicleName), vehicleName, StringComparison.CurrentCultureIgnoreCase))
            .Select(file =>
            {
                var linkedCustomerNames = file.CustomerLinks
                    .Select(link => TrimToNull(link.Customer?.Name))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.CurrentCulture)
                    .ToList();
                var sourceSummary = linkedCustomerNames.Count > 0
                    ? string.Join(", ", linkedCustomerNames)
                    : "차량별 자료";

                return new FileItemViewModel
                {
                    Id = file.Id,
                    FileName = file.DisplayName,
                    FilePath = file.FilePath,
                    FileType = GetDisplayFileType(file.FileType, file.CustomFileType),
                    FileOrder = file.FileOrder,
                    Summary = $"{sourceSummary} · {file.CreatedAt:yyyy-MM-dd}",
                    PreviewTitle = file.DisplayName,
                    PreviewMeta = $"{sourceSummary} · {file.CreatedAt:yyyy-MM-dd}",
                    PreviewLabel = file.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : GetDisplayFileType(file.FileType, file.CustomFileType),
                    CreatedAt = file.CreatedAt,
                };
            });

        var estimates = customerEstimates
            .Concat(vehicleResources)
            .OrderByDescending(file => file.CreatedAt)
            .Take(10)
            .ToList();

        ReferenceEstimateTitleTextBlock.Text = estimates.Count == 0
            ? $"차량별 자료 없음 · {vehicleName}"
            : $"차량별 자료 {estimates.Count}건 · {vehicleName}";
        ReferenceEstimateListBox.ItemsSource = estimates;
    }

    private void RefreshFileTypeFilterOptions()
    {
        var selectedFilter = FileTypeFilterComboBox.SelectedItem?.ToString();

        FileTypeFilterComboBox.Items.Clear();
        FileTypeFilterComboBox.Items.Add(AllFileTypesFilter);

        var fileTypes = GetSelectedCustomer()?.Files
            .Select(file => file.FileType)
            .Where(fileType => !string.IsNullOrWhiteSpace(fileType))
            .Distinct(StringComparer.CurrentCulture)
            .OrderBy(fileType => fileType);

        if (fileTypes is not null)
        {
            foreach (var fileType in fileTypes)
            {
                FileTypeFilterComboBox.Items.Add(fileType);
            }
        }

        FileTypeFilterComboBox.SelectedItem = selectedFilter is not null && FileTypeFilterComboBox.Items.Contains(selectedFilter)
            ? selectedFilter
            : AllFileTypesFilter;
    }

    private void ApplyFileListView()
    {
        if (FileListBox.ItemsSource is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(FileListBox.ItemsSource);
        if (view is null)
        {
            return;
        }

        var selectedFilter = FileTypeFilterComboBox.SelectedItem?.ToString();
        view.Filter = item =>
            selectedFilter is null ||
            selectedFilter == AllFileTypesFilter ||
            item is FileItemViewModel file && file.FileType == selectedFilter;

        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();

            switch (FileSortComboBox.SelectedItem?.ToString())
            {
                case "최신순":
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.CreatedAt), ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.FileName), ListSortDirection.Ascending));
                    break;
                case "파일명순":
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.FileName), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.CreatedAt), ListSortDirection.Descending));
                    break;
                default:
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.FileType), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.FileOrder), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(FileItemViewModel.CreatedAt), ListSortDirection.Descending));
                    break;
            }
        }
    }

    private void SelectFirstVisibleFileIfNeeded()
    {
        var viewModel = GetViewModel();
        if (viewModel is null || FileListBox.ItemsSource is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(FileListBox.ItemsSource);
        if (view is null)
        {
            return;
        }

        if (viewModel.SelectedFile is not null && view.Contains(viewModel.SelectedFile))
        {
            return;
        }

        viewModel.SelectedFile = view.Cast<FileItemViewModel>().FirstOrDefault();
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

    private static string GetCustomerFilesDirectory(int customerId)
    {
        return Path.Combine(AppPaths.CustomersDirectory, customerId.ToString(CultureInfo.InvariantCulture), "files");
    }

    private void SyncEstimateVehicleResourceLink(
        AppDbContext dbContext,
        int customerId,
        string customerName,
        CustomerFile customerFile,
        string? vehicleBrand,
        string? vehicleName,
        string? fuelType,
        string? capitalCompany,
        string? rentalCompany,
        DateTime now)
    {
        if (IsEstimateFileType(customerFile.FileType, customerFile.CustomFileType))
        {
            LinkEstimateFileToVehicleResource(
                dbContext,
                customerId,
                customerName,
                customerFile,
                vehicleBrand,
                vehicleName,
                fuelType,
                capitalCompany,
                rentalCompany,
                now);
            return;
        }

        RemoveVehicleResourcesLinkedToCustomerFile(dbContext, customerFile);
    }

    private void LinkEstimateFileToVehicleResource(
        AppDbContext dbContext,
        int customerId,
        string customerName,
        CustomerFile customerFile,
        string? vehicleBrand,
        string? vehicleName,
        string? fuelType,
        string? capitalCompany,
        string? rentalCompany,
        DateTime now)
    {
        if (!IsEstimateFileType(customerFile.FileType, customerFile.CustomFileType))
        {
            return;
        }

        var existingLink = dbContext.CustomerVehicleResourceLinks
            .Include(link => link.VehicleResourceFile)
            .FirstOrDefault(link => link.CustomerFileId == customerFile.Id);

        vehicleBrand = TrimToNull(vehicleBrand);
        vehicleName = TrimToNull(vehicleName);
        fuelType = TrimToNull(fuelType);
        capitalCompany = null;
        rentalCompany = TrimToNull(rentalCompany);
        var fileOrder = existingLink?.VehicleResourceFile?.FileOrder
            ?? GetNextVehicleResourceFileOrder(dbContext, customerFile.FileType, customerFile.CustomFileType);
        var displayName = BuildVehicleResourceDisplayName(vehicleName, customerFile.FileType, customerFile.CustomFileType, fileOrder);

        var resource = existingLink?.VehicleResourceFile ?? new VehicleResourceFile
        {
            CreatedAt = now,
        };

        resource.OriginalFileName = customerFile.OriginalFileName;
        resource.StoredFileName = customerFile.StoredFileName;
        resource.DisplayName = displayName;
        resource.FilePath = customerFile.FilePath;
        resource.FileType = customerFile.FileType;
        resource.CustomFileType = customerFile.CustomFileType;
        resource.FileOrder = fileOrder;
        resource.VehicleBrand = vehicleBrand;
        resource.VehicleName = vehicleName;
        resource.FuelType = fuelType;
        resource.CapitalCompany = capitalCompany;
        resource.RentalCompany = rentalCompany;
        resource.Memo = customerFile.Memo;

        if (existingLink is null)
        {
            dbContext.VehicleResourceFiles.Add(resource);
            dbContext.CustomerVehicleResourceLinks.Add(new CustomerVehicleResourceLink
            {
                CustomerId = customerId,
                VehicleResourceFile = resource,
                CustomerFile = customerFile,
                Memo = $"{customerName} 견적",
                CreatedAt = now,
            });
        }
        else
        {
            existingLink.CustomerId = customerId;
            existingLink.CustomerFile = customerFile;
            existingLink.Memo = $"{customerName} 견적";
        }
    }

    private static void RemoveVehicleResourcesLinkedToCustomerFile(AppDbContext dbContext, CustomerFile customerFile)
    {
        var links = dbContext.CustomerVehicleResourceLinks
            .Include(link => link.VehicleResourceFile)
            .Where(link => link.CustomerFileId == customerFile.Id)
            .ToList();

        foreach (var link in links)
        {
            var resource = link.VehicleResourceFile;
            var linkCount = resource is null
                ? 0
                : dbContext.CustomerVehicleResourceLinks.Count(item => item.VehicleResourceFileId == resource.Id);

            dbContext.CustomerVehicleResourceLinks.Remove(link);

            if (resource is not null && linkCount <= 1)
            {
                dbContext.VehicleResourceFiles.Remove(resource);
            }
        }
    }

    private void TryDeleteStoredCustomerFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"파일 정보는 삭제했지만 실제 파일 삭제에 실패했습니다.\n\n{filePath}\n\n{ex.Message}",
                "Consult Note",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private string? GetSelectedVehicleNameForSearch()
    {
        return VehicleNameComboBox.SelectedItem switch
        {
            ComboBoxItem item => TrimToNull(item.Content?.ToString()),
            string text => TrimToNull(text),
            object value => TrimToNull(value.ToString()),
            _ => null,
        };
    }

    private static string BuildCustomerFileDisplayName(string customerName, string fileType, string? customFileType, int fileOrder)
    {
        var displayFileType = fileType == "기타" && !string.IsNullOrWhiteSpace(customFileType)
            ? customFileType.Trim()
            : fileType.Trim();

        return $"{customerName.Trim()}_{displayFileType}_{fileOrder}";
    }

    private static string BuildCustomerStoredFileBaseName(
        string customerName,
        string fileType,
        string? customFileType,
        int fileOrder,
        string? memo)
    {
        var baseName = BuildCustomerFileDisplayName(customerName, fileType, customFileType, fileOrder);
        return IsEstimateFileType(fileType, customFileType) && !string.IsNullOrWhiteSpace(memo)
            ? $"{baseName}_{memo.Trim()}"
            : baseName;
    }

    private static string GetDisplayFileType(string fileType, string? customFileType)
    {
        return fileType == "기타" && !string.IsNullOrWhiteSpace(customFileType)
            ? customFileType.Trim()
            : fileType.Trim();
    }

    private static bool IsEstimateFileType(string fileType, string? customFileType)
    {
        return string.Equals(GetDisplayFileType(fileType, customFileType), "견적", StringComparison.CurrentCultureIgnoreCase);
    }

    private static string BuildVehicleResourceDisplayName(string? vehicleName, string fileType, string? customFileType, int fileOrder)
    {
        var safeVehicleName = string.IsNullOrWhiteSpace(vehicleName) ? "차량별자료" : vehicleName.Trim();
        return $"{safeVehicleName}_{GetDisplayFileType(fileType, customFileType)}_{fileOrder}";
    }

    private static int GetNextFileOrderFromDatabase(
        AppDbContext dbContext,
        int customerId,
        string fileType,
        string? customFileType,
        int? excludingFileId = null)
    {
        var displayFileType = GetDisplayFileType(fileType, customFileType);
        var maxOrder = dbContext.CustomerFiles
            .Where(file => file.CustomerId == customerId)
            .AsEnumerable()
            .Where(file => excludingFileId is null || file.Id != excludingFileId.Value)
            .Where(file => string.Equals(
                GetDisplayFileType(file.FileType, file.CustomFileType),
                displayFileType,
                StringComparison.CurrentCulture))
            .Select(file => (int?)file.FileOrder)
            .Max();

        return (maxOrder ?? 0) + 1;
    }

    private static int GetNextVehicleResourceFileOrder(
        AppDbContext dbContext,
        string fileType,
        string? customFileType)
    {
        var displayFileType = GetDisplayFileType(fileType, customFileType);
        var maxOrder = dbContext.VehicleResourceFiles
            .AsNoTracking()
            .AsEnumerable()
            .Where(file => string.Equals(
                GetDisplayFileType(file.FileType, file.CustomFileType),
                displayFileType,
                StringComparison.CurrentCulture))
            .Select(file => (int?)file.FileOrder)
            .Max();

        return (maxOrder ?? 0) + 1;
    }

    private static string BuildStoredCustomerFileName(string directoryPath, string displayName, string extension)
    {
        var safeBaseName = SanitizeFileName(displayName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.ToLowerInvariant();
        var storedFileName = $"{safeBaseName}{safeExtension}";
        var destinationPath = Path.Combine(directoryPath, storedFileName);
        var suffix = 2;

        while (File.Exists(destinationPath))
        {
            storedFileName = $"{safeBaseName}({suffix}){safeExtension}";
            destinationPath = Path.Combine(directoryPath, storedFileName);
            suffix++;
        }

        return storedFileName;
    }

    private static Dictionary<string, int> GetNextFileOrdersByType(CustomerItemViewModel customer)
    {
        return customer.Files
            .GroupBy(file => file.FileType)
            .ToDictionary(
                group => group.Key,
                group => Math.Max(1, group.Max(file => file.FileOrder) + 1));
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
        SetPlaceholderTextOrValue(VehicleDetailTextBox, customer.FormVehicleDetail);
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
        RentRadioButton.IsChecked = false;
        LeaseRadioButton.IsChecked = false;
        VehicleBrandComboBox.SelectedItem = null;
        VehicleBrandComboBox.Text = string.Empty;
        VehicleNameComboBox.SelectedItem = null;
        VehicleNameComboBox.Text = string.Empty;
        FuelTypeComboBox.SelectedItem = null;
        FuelTypeComboBox.Text = string.Empty;
        SetPlaceholder(VehicleDetailTextBox);
        ContractPeriodComboBox.SelectedIndex = -1;
        ContractPeriodComboBox.Text = string.Empty;
        SetMileageControl(null);
        DeliveryRegionComboBox.SelectedIndex = -1;
        DeliveryRegionComboBox.Text = string.Empty;
        SetPlaceholder(DeliveryRegionDetailTextBox);
        NoInitialCostCheckBox.IsChecked = false;
        PrepaymentTextBox.Text = string.Empty;
        DepositTextBox.Text = string.Empty;
        PersonalOwnerRadioButton.IsChecked = false;
        SoleProprietorOwnerRadioButton.IsChecked = false;
        CorporateOwnerRadioButton.IsChecked = false;
        BusinessExperienceToggleButton.IsChecked = false;
        ActualDriverTextBox.Text = string.Empty;
        DriverLicenseToggleButton.IsChecked = false;
        InsuranceAgeComboBox.SelectedIndex = -1;
        InsuranceAgeComboBox.Text = string.Empty;
        CreditStatusTextBox.Text = string.Empty;
        SpecialNoteTextBox.Text = string.Empty;
        ContractHolderSameAsCustomerCheckBox.IsChecked = false;
        ContractHolderNameTextBox.Text = string.Empty;
        ContractHolderPhoneTextBox.Text = string.Empty;
        ContractHolderNameTextBox.IsEnabled = true;
        ContractHolderPhoneTextBox.IsEnabled = true;
        UpdateContractHolderVisibility();
    }

    private string GetOwnerType()
    {
        if (PersonalOwnerRadioButton.IsChecked == true)
        {
            return "개인";
        }

        if (SoleProprietorOwnerRadioButton.IsChecked == true)
        {
            return "개인사업자";
        }

        if (CorporateOwnerRadioButton.IsChecked == true)
        {
            return "법인사업자";
        }

        return string.Empty;
    }

    private string GetContractType()
    {
        if (RentRadioButton.IsChecked == true)
        {
            return "렌트";
        }

        if (LeaseRadioButton.IsChecked == true)
        {
            return "리스";
        }

        return string.Empty;
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
            CleanPlaceholder(DeliveryRegionDetailTextBox.Text, DeliveryRegionDetailPlaceholder),
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private string GetInitialCostText()
    {
        return NoInitialCostCheckBox.IsChecked == true
            ? "없음"
            : string.Join(" / ", new[]
            {
                TrimToNull(PrepaymentTextBox.Text),
                TrimToNull(DepositTextBox.Text),
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

    private static string GetPlaceholder(TextBox textBox)
    {
        return textBox.Name switch
        {
            nameof(VehicleDetailTextBox) => VehicleDetailPlaceholder,
            nameof(DeliveryRegionDetailTextBox) => DeliveryRegionDetailPlaceholder,
            _ => string.Empty,
        };
    }

    private static void SetPlaceholder(TextBox textBox)
    {
        var placeholder = GetPlaceholder(textBox);
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            return;
        }

        textBox.Text = placeholder;
        textBox.Foreground = SystemColors.GrayTextBrush;
    }

    private static void SetPlaceholderTextOrValue(TextBox textBox, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetPlaceholder(textBox);
            return;
        }

        textBox.Text = value;
        textBox.Foreground = SystemColors.ControlTextBrush;
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
            MileageComboBox.SelectedIndex = -1;
            MileageComboBox.Text = string.Empty;
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
            SetPlaceholder(DeliveryRegionDetailTextBox);
            return;
        }

        var parts = deliveryRegion.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        SetComboText(DeliveryRegionComboBox, parts[0]);
        SetPlaceholderTextOrValue(DeliveryRegionDetailTextBox, parts.Length > 1 ? parts[1] : null);
    }

    private void SetInitialCost(string? initialCost)
    {
        if (string.IsNullOrWhiteSpace(initialCost))
        {
            NoInitialCostCheckBox.IsChecked = false;
            PrepaymentTextBox.Text = string.Empty;
            DepositTextBox.Text = string.Empty;
            return;
        }

        NoInitialCostCheckBox.IsChecked = initialCost == "없음";
        if (NoInitialCostCheckBox.IsChecked == true)
        {
            PrepaymentTextBox.Text = string.Empty;
            DepositTextBox.Text = string.Empty;
            return;
        }

        var parts = initialCost.Split(" / ", StringSplitOptions.TrimEntries);
        PrepaymentTextBox.Text = parts.ElementAtOrDefault(0) ?? string.Empty;
        DepositTextBox.Text = parts.ElementAtOrDefault(1) ?? string.Empty;
    }

    private void SetOwnerType(string? ownerType)
    {
        PersonalOwnerRadioButton.IsChecked = ownerType == "개인";
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

    private static void LogUiError(Exception exception, string fileName)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogsDirectory);
            File.WriteAllText(Path.Combine(AppPaths.LogsDirectory, fileName), exception.ToString());
        }
        catch
        {
            // Keep the user-facing message path alive even if logging fails.
        }
    }
}
