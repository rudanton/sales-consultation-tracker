using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsultNote;

public partial class VehicleManagementDialog : Window
{
    private VehicleListItem? _selectedVehicle;
    private bool _isNewVehicle;

    public VehicleManagementDialog()
    {
        InitializeComponent();
        DataContext = this;
        LoadVehicles();
    }

    public ObservableCollection<VehicleListItem> Vehicles { get; } = [];

    private void LoadVehicles(int? selectedVehicleId = null)
    {
        using var dbContext = new AppDbContext();
        var vehicles = dbContext.Vehicles
            .AsNoTracking()
            .OrderByDescending(vehicle => vehicle.IsActive)
            .ThenBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Name)
            .Select(vehicle => new VehicleListItem(vehicle))
            .ToList();

        Vehicles.Clear();
        foreach (var vehicle in vehicles)
        {
            Vehicles.Add(vehicle);
        }

        VehiclesListBox.SelectedItem = selectedVehicleId is null
            ? Vehicles.FirstOrDefault()
            : Vehicles.FirstOrDefault(vehicle => vehicle.Id == selectedVehicleId) ?? Vehicles.FirstOrDefault();
    }

    private void VehiclesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedVehicle = VehiclesListBox.SelectedItem as VehicleListItem;
        _isNewVehicle = false;
        LoadEditor(_selectedVehicle);
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedVehicle = null;
        _isNewVehicle = true;
        VehiclesListBox.SelectedItem = null;
        LoadEditor(null);
        BrandTextBox.Focus();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = TrimToNull(NameTextBox.Text);
        if (name is null)
        {
            MessageBox.Show("차량명을 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var now = DateTime.Now;
        using var dbContext = new AppDbContext();
        Vehicle vehicle;

        if (_isNewVehicle || _selectedVehicle is null)
        {
            vehicle = new Vehicle
            {
                CreatedAt = now,
                IsActive = true,
            };
            dbContext.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle = dbContext.Vehicles.First(item => item.Id == _selectedVehicle.Id);
        }

        vehicle.Brand = TrimToNull(BrandTextBox.Text);
        vehicle.Name = name;
        vehicle.FuelTypes = TrimToNull(FuelTypesTextBox.Text);
        vehicle.Memo = TrimToNull(MemoTextBox.Text);
        vehicle.UpdatedAt = now;

        dbContext.SaveChanges();
        _isNewVehicle = false;
        LoadVehicles(vehicle.Id);
    }

    private void ToggleActiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedVehicle is null)
        {
            return;
        }

        using var dbContext = new AppDbContext();
        var vehicle = dbContext.Vehicles.FirstOrDefault(item => item.Id == _selectedVehicle.Id);
        if (vehicle is null)
        {
            return;
        }

        vehicle.IsActive = !vehicle.IsActive;
        vehicle.UpdatedAt = DateTime.Now;
        dbContext.SaveChanges();
        LoadVehicles(vehicle.Id);
    }

    private void LoadEditor(VehicleListItem? vehicle)
    {
        BrandTextBox.Text = vehicle?.Brand ?? string.Empty;
        NameTextBox.Text = vehicle?.Name ?? string.Empty;
        FuelTypesTextBox.Text = vehicle?.FuelTypes ?? string.Empty;
        MemoTextBox.Text = vehicle?.Memo ?? string.Empty;
        ToggleActiveButton.IsEnabled = vehicle is not null;
        ToggleActiveButton.Content = vehicle?.IsActive == false ? "활성화" : "비활성화";
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public sealed class VehicleListItem : INotifyPropertyChanged
    {
        public VehicleListItem(Vehicle vehicle)
        {
            Id = vehicle.Id;
            Brand = vehicle.Brand;
            Name = vehicle.Name;
            FuelTypes = vehicle.FuelTypes;
            Memo = vehicle.Memo;
            IsActive = vehicle.IsActive;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; }

        public string? Brand { get; }

        public string Name { get; }

        public string? FuelTypes { get; }

        public string? Memo { get; }

        public bool IsActive { get; }

        public string DisplayName => string.IsNullOrWhiteSpace(Brand) ? Name : $"{Brand} {Name}";

        public string ActiveText => IsActive ? "사용" : "숨김";

        public string Summary => string.Join(" · ", new[] { FuelTypes, Memo }.Where(value => !string.IsNullOrWhiteSpace(value)));

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
