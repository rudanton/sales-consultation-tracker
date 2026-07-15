using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using ConsultNote.Infrastructure;
using ConsultNote.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConsultNote;

public partial class VehicleResourceManagementDialog : Window, INotifyPropertyChanged
{
    private VehicleResourceFileItemViewModel? _selectedResource;

    public VehicleResourceManagementDialog()
    {
        InitializeComponent();
        DataContext = this;
        PreviewKeyDown += VehicleResourceManagementDialog_PreviewKeyDown;
        LoadResources();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<VehicleResourceFileItemViewModel> ResourceFiles { get; } = [];

    public VehicleResourceFileItemViewModel? SelectedResource
    {
        get => _selectedResource;
        set
        {
            if (!Equals(_selectedResource, value))
            {
                _selectedResource = value;
                OnPropertyChanged();
            }
        }
    }

    private void LoadResources(int? selectedResourceId = null)
    {
        using var dbContext = new AppDbContext();
        var resources = dbContext.VehicleResourceFiles
            .AsNoTracking()
            .AsEnumerable()
            .OrderBy(file => file.VehicleName)
            .ThenBy(file => GetDisplayFileType(file.FileType, file.CustomFileType))
            .ThenBy(file => file.FileOrder)
            .ThenByDescending(file => file.CreatedAt)
            .Select(ToViewModel)
            .ToList();

        ResourceFiles.Clear();
        foreach (var resource in resources)
        {
            ResourceFiles.Add(resource);
        }

        SelectedResource = selectedResourceId is null
            ? ResourceFiles.FirstOrDefault()
            : ResourceFiles.FirstOrDefault(resource => resource.Id == selectedResourceId) ?? ResourceFiles.FirstOrDefault();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddVehicleResource(sourceFilePath: null);
    }

    private void VehicleResourceManagementDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V && Clipboard.ContainsImage())
        {
            e.Handled = true;
            AddClipboardImageAsVehicleResource();
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        EditSelectedResource();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedResource();
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = GetVehicleResourcesDirectory();
            Directory.CreateDirectory(directory);
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"차량별 자료 폴더를 열 수 없습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void VehicleResourceListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (SelectedResource is null)
        {
            return;
        }

        OpenResourceFile(SelectedResource);
    }

    private void VehicleResourceDialog_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = CanAcceptDroppedFiles(e) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void VehicleResourceDialog_Drop(object sender, DragEventArgs e)
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
            if (!AddVehicleResource(filePath))
            {
                break;
            }
        }

        e.Handled = true;
    }

    private static bool CanAcceptDroppedFiles(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        var filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
        return filePaths?.Any(filePath => File.Exists(filePath) && AddCustomerFileDialog.IsSupportedFile(filePath)) == true;
    }

    private bool AddVehicleResource(string? sourceFilePath)
    {
        if (sourceFilePath is not null && !AddCustomerFileDialog.IsSupportedFile(sourceFilePath))
        {
            MessageBox.Show(".jpg, .jpeg, .png, .pdf 파일만 추가할 수 있습니다.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        using var dbContext = new AppDbContext();
        var dialog = new AddVehicleResourceFileDialog(GetNextOrdersByFileType(dbContext), sourceFilePath: sourceFilePath)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        var now = DateTime.Now;
        var resourceOrder = GetNextFileOrderFromDatabase(dbContext, dialog.FileType, dialog.CustomFileType);
        var displayName = BuildDisplayName(dialog.VehicleName, dialog.FileType, dialog.CustomFileType, resourceOrder);
        var storedBaseName = string.IsNullOrWhiteSpace(dialog.Memo)
            ? displayName
            : $"{displayName}_{dialog.Memo.Trim()}";
        var directory = GetVehicleResourcesDirectory();
        var storedFileName = string.Empty;
        var destinationPath = string.Empty;

        try
        {
            Directory.CreateDirectory(directory);
            var extension = Path.GetExtension(dialog.SourceFilePath);
            storedFileName = BuildStoredFileName(directory, storedBaseName, extension);
            destinationPath = Path.Combine(directory, storedFileName);
            File.Copy(dialog.SourceFilePath, destinationPath, overwrite: false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일 복사 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        try
        {
            var resource = new VehicleResourceFile
            {
                OriginalFileName = dialog.OriginalFileName,
                StoredFileName = storedFileName,
                DisplayName = displayName,
                FilePath = destinationPath,
                FileType = dialog.FileType,
                CustomFileType = dialog.FileType == "기타" ? dialog.CustomFileType : null,
                FileOrder = resourceOrder,
                VehicleBrand = dialog.VehicleBrand,
                VehicleName = dialog.VehicleName,
                FuelType = dialog.FuelType,
                CapitalCompany = null,
                RentalCompany = dialog.RentalCompany,
                Memo = dialog.Memo,
                CreatedAt = now,
            };

            dbContext.VehicleResourceFiles.Add(resource);
            dbContext.SaveChanges();
            LoadResources(resource.Id);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"저장 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void AddClipboardImageAsVehicleResource()
    {
        string? tempFilePath = null;
        try
        {
            var image = Clipboard.GetImage();
            if (image is null)
            {
                return;
            }

            var tempDirectory = Path.Combine(AppPaths.StorageDirectory, "clipboard");
            Directory.CreateDirectory(tempDirectory);
            tempFilePath = Path.Combine(tempDirectory, $"{DateTime.Now:yyyyMMdd_HHmmssfff}_vehicle_resource_clipboard.png");

            using (var stream = File.Create(tempFilePath))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }

            AddVehicleResource(tempFilePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"클립보드 이미지 추가 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            if (tempFilePath is not null)
            {
                TryDeleteStoredFile(tempFilePath);
            }
        }
    }

    private void EditSelectedResource()
    {
        if (SelectedResource is null)
        {
            MessageBox.Show("수정할 차량별 자료를 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        using var dbContext = new AppDbContext();
        var resource = dbContext.VehicleResourceFiles.FirstOrDefault(file => file.Id == SelectedResource.Id);
        if (resource is null)
        {
            return;
        }

        var dialog = new AddVehicleResourceFileDialog(
            GetNextOrdersByFileType(dbContext),
            resource.FileType,
            resource.CustomFileType,
            resource.FileOrder,
            resource.VehicleBrand,
            resource.VehicleName,
            resource.FuelType,
            resource.CapitalCompany,
            resource.RentalCompany,
            resource.Memo,
            isEditMode: true)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var originalDisplayFileType = GetDisplayFileType(resource.FileType, resource.CustomFileType);
        var editedDisplayFileType = GetDisplayFileType(dialog.FileType, dialog.CustomFileType);
        var fileOrder = string.Equals(originalDisplayFileType, editedDisplayFileType, StringComparison.CurrentCulture)
            ? resource.FileOrder
            : GetNextFileOrderFromDatabase(dbContext, dialog.FileType, dialog.CustomFileType, resource.Id);

        resource.FileType = dialog.FileType;
        resource.CustomFileType = dialog.FileType == "기타" ? dialog.CustomFileType : null;
        resource.FileOrder = fileOrder;
        resource.VehicleBrand = dialog.VehicleBrand;
        resource.VehicleName = dialog.VehicleName;
        resource.FuelType = dialog.FuelType;
        resource.CapitalCompany = null;
        resource.RentalCompany = dialog.RentalCompany;
        resource.Memo = dialog.Memo;
        resource.DisplayName = BuildDisplayName(dialog.VehicleName, dialog.FileType, dialog.CustomFileType, fileOrder);

        try
        {
            dbContext.SaveChanges();
            LoadResources(resource.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"저장 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteSelectedResource()
    {
        if (SelectedResource is null)
        {
            MessageBox.Show("삭제할 차량별 자료를 선택해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"선택한 차량별 자료를 삭제할까요?\n\n{SelectedResource.FileName}",
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
            var resource = dbContext.VehicleResourceFiles.FirstOrDefault(file => file.Id == SelectedResource.Id);
            if (resource is null)
            {
                return;
            }

            var filePath = resource.FilePath;
            dbContext.VehicleResourceFiles.Remove(resource);
            dbContext.SaveChanges();
            TryDeleteStoredFile(filePath);
            LoadResources();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"삭제 중 오류가 발생했습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void OpenResourceFile(VehicleResourceFileItemViewModel resource)
    {
        if (!File.Exists(resource.FilePath))
        {
            MessageBox.Show($"파일 경로가 존재하지 않습니다.\n\n{resource.FilePath}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(resource.FilePath)
            {
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"파일을 열 수 없습니다.\n\n{ex.Message}", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static VehicleResourceFileItemViewModel ToViewModel(VehicleResourceFile file)
    {
        var displayFileType = GetDisplayFileType(file.FileType, file.CustomFileType);
        var rentalCompany = string.IsNullOrWhiteSpace(file.RentalCompany) ? file.CapitalCompany : file.RentalCompany;
        var vehicleSummary = string.Join(" · ", new[] { file.VehicleBrand, file.VehicleName, file.FuelType, rentalCompany }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return new VehicleResourceFileItemViewModel
        {
            Id = file.Id,
            FileName = file.DisplayName,
            FilePath = file.FilePath,
            FileType = displayFileType,
            FileOrder = file.FileOrder,
            VehicleSummary = string.IsNullOrWhiteSpace(vehicleSummary) ? "-" : vehicleSummary,
            Summary = string.Join(" · ", new[] { file.Memo, file.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture) }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            CreatedAt = file.CreatedAt,
        };
    }

    private static Dictionary<string, int> GetNextOrdersByFileType(AppDbContext dbContext)
    {
        return dbContext.VehicleResourceFiles
            .AsNoTracking()
            .AsEnumerable()
            .GroupBy(file => GetDisplayFileType(file.FileType, file.CustomFileType))
            .ToDictionary(
                group => group.Key,
                group => Math.Max(1, group.Max(file => file.FileOrder) + 1));
    }

    private static int GetNextFileOrderFromDatabase(
        AppDbContext dbContext,
        string fileType,
        string? customFileType,
        int? excludingFileId = null)
    {
        var displayFileType = GetDisplayFileType(fileType, customFileType);
        var maxOrder = dbContext.VehicleResourceFiles
            .AsNoTracking()
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

    private static string BuildDisplayName(string? vehicleName, string fileType, string? customFileType, int fileOrder)
    {
        var safeVehicleName = string.IsNullOrWhiteSpace(vehicleName) ? "차량자료" : vehicleName.Trim();
        return $"{safeVehicleName}_{GetDisplayFileType(fileType, customFileType)}_{fileOrder}";
    }

    private static string GetDisplayFileType(string fileType, string? customFileType)
    {
        return fileType == "기타" && !string.IsNullOrWhiteSpace(customFileType)
            ? customFileType.Trim()
            : fileType.Trim();
    }

    private static string GetVehicleResourcesDirectory()
    {
        return AppPaths.VehicleResourcesDirectory;
    }

    private static string BuildStoredFileName(string directoryPath, string displayName, string extension)
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

    private static void TryDeleteStoredFile(string filePath)
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

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(fileName) ? "vehicle-resource" : fileName;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
