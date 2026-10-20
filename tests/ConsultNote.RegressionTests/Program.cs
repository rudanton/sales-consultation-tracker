using System.IO;
using System.Windows;
using System.Windows.Controls;
using ConsultNote;
using ConsultNote.Data;
using ConsultNote.Data.Entities;
using ConsultNote.Infrastructure;
using ConsultNote.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

internal static class Program
{
    private static int _checks;

    [STAThread]
    private static int Main()
    {
        try
        {
            // The unique artifacts output directory keeps these fixtures away from user data.
            Check(!File.Exists(AppPaths.DatabasePath), "Use a fresh --artifacts-path for this run.");
            var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
            TestUpgrade();
            AppStartup.Initialize();
            AppStartup.Initialize();
            SeedFixtures();
            TestCatalog();
            TestCustomerLists();
            TestFilters();
            TestDialogs();
            Console.WriteLine($"PASS: {_checks} regression checks.");
            app.Shutdown();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void TestUpgrade()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        db.GetService<IMigrator>().Migrate("20260715000000_AddConsultationLogStatus");
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE "VehicleResourceFiles" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "OriginalFileName" TEXT NOT NULL, "StoredFileName" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL, "FilePath" TEXT NOT NULL, "FileType" TEXT NOT NULL,
                "CustomFileType" TEXT NULL, "FileOrder" INTEGER NOT NULL DEFAULT 1,
                "VehicleBrand" TEXT NULL, "VehicleName" TEXT NULL, "FuelType" TEXT NULL,
                "CapitalCompany" TEXT NULL, "RentalCompany" TEXT NULL, "Memo" TEXT NULL, "CreatedAt" TEXT NOT NULL
            );
            """);
        db.Database.ExecuteSqlRaw("""
            INSERT INTO "VehicleResourceFiles"
                ("OriginalFileName", "StoredFileName", "DisplayName", "FilePath", "FileType", "FileOrder", "CapitalCompany", "CreatedAt")
            VALUES ('old.png', 'old.png', 'legacy', 'old.png', '견적', 1, '  Legacy Capital  ', '2026-10-01');
            """);
        db.Database.Migrate();
        RentalCompanyCatalog.ImportExisting(db);
        var resource = db.VehicleResourceFiles.Single();
        Check(resource.QuoteType is null, "Legacy quote type must remain unspecified.");
        Check(resource.RentalCompany == "Legacy Capital", "Legacy company must be imported and trimmed.");
        Check(RentalCompanyCatalog.Load(db).SequenceEqual(new[] { "Legacy Capital" }), "Legacy catalog import.");
        resource.QuoteType = "리스";
        db.SaveChanges();
        db.ChangeTracker.Clear();
        Check(db.VehicleResourceFiles.Single().QuoteType == "리스", "Quote type round trip after migration.");
        Check(!db.Database.GetPendingMigrations().Any(), "All migrations must apply.");
    }

    private static void SeedFixtures()
    {
        using var db = new AppDbContext();
        var now = DateTime.Now;
        foreach (var status in new[] { CustomerStatus.Consulting, CustomerStatus.Delivered, CustomerStatus.Discarded })
        {
            var customer = new Customer { Name = status.ToString(), Status = status, CreatedAt = now, UpdatedAt = now };
            customer.ConsultationLogs.Add(new ConsultationLog
            {
                Content = "Historical consultation", Status = CustomerStatus.NoAnswer, CreatedAt = now, UpdatedAt = now,
            });
            db.Customers.Add(customer);
        }

        db.VehicleResourceFiles.AddRange(
            Resource("rent", "가솔린", "렌트", "테스트캐피탈"),
            Resource("lease", "하이브리드", "리스", "테스트캐피탈"),
            Resource("legacy", "하이브리드", null, "두번째캐피탈"),
            Resource("other", "전기", null, null, "차량설명"));
        db.SaveChanges();
        RentalCompanyCatalog.ImportExisting(db);
    }

    private static VehicleResourceFile Resource(string name, string fuel, string? quoteType, string? company, string fileType = "견적")
    {
        return new VehicleResourceFile
        {
            DisplayName = name, OriginalFileName = name + ".png", StoredFileName = name + ".png",
            FilePath = name + ".png", FileType = fileType, VehicleBrand = "현대", VehicleName = "투싼",
            FuelType = fuel, QuoteType = quoteType, RentalCompany = company, CreatedAt = DateTime.Now,
        };
    }

    private static void TestCatalog()
    {
        using var db = new AppDbContext();
        Check(RentalCompanyCatalog.Load(db).Count == 2, "Existing company list imported.");
        Check(RentalCompanyCatalog.Add(db, "  New Capital  ") == "New Capital", "Trim new company.");
        Check(RentalCompanyCatalog.Add(db, "new capital") == "New Capital", "Reuse canonical company name.");
        Check(RentalCompanyCatalog.Load(db).Count == 3, "Duplicate company not inserted.");
        try
        {
            RentalCompanyCatalog.Add(db, "   ");
            throw new InvalidOperationException("Blank company was accepted.");
        }
        catch (ArgumentException)
        {
            Check(true, "Reject blank company.");
        }
    }

    private static void TestCustomerLists()
    {
        var vm = new MainWindowViewModel();
        Check(vm.Customers.Count == 1 && vm.Customers[0].Status == CustomerStatus.Consulting, "Only active customers in main list.");
        Check(vm.DashboardSummary.StartsWith("활성 1명"), "Delivered customer excluded from active count.");
        Check(vm.DashboardStatusSummary.Contains("인도완료 1"), "Delivered count retained.");
        vm.ShowDiscardedCustomers = true;
        Check(vm.Customers.Count == 2, "Delivered and discarded in archive.");
        vm.SelectedCustomer = vm.Customers.Single(customer => customer.Status == CustomerStatus.Delivered);
        Check(vm.IsSelectedCustomerDiscarded, "Delivered customer can be restored.");
        vm.SelectedCustomerStatusFilter = vm.CustomerStatusFilterOptions.Single(option => option.Status == CustomerStatus.Delivered);
        Check(vm.Customers.Count == 1 && vm.Customers[0].Status == CustomerStatus.Delivered, "Filter delivered archive.");
        vm.ShowDiscardedCustomers = false;
        Check(vm.Customers.Count == 1, "Switching list resets status filter.");

        using var db = new AppDbContext();
        Check(db.Customers.Single(customer => customer.Status == CustomerStatus.Delivered).Status == CustomerStatus.Delivered,
            "Delivered status retained.");
        Check(db.ConsultationLogs.All(log => log.Status == CustomerStatus.NoAnswer), "Historical statuses unchanged.");
    }

    private static void TestFilters()
    {
        using var db = new AppDbContext();
        Check(new VehicleResourceFilter().Apply(db.VehicleResourceFiles).Count() == 4, "All resources.");
        Check(new VehicleResourceFilter(FuelType: "하이브리드").Apply(db.VehicleResourceFiles).Count() == 2, "Fuel filter.");
        Check(new VehicleResourceFilter(QuoteType: "렌트").Apply(db.VehicleResourceFiles).Single().DisplayName == "rent", "Rent filter.");
        Check(new VehicleResourceFilter(QuoteType: "리스").Apply(db.VehicleResourceFiles).Single().DisplayName == "lease", "Lease filter.");
        Check(new VehicleResourceFilter(QuoteType: VehicleResourceFilter.UnspecifiedQuoteType).Apply(db.VehicleResourceFiles)
            .Single().DisplayName == "legacy", "Unspecified includes old quotes, excludes other documents.");
        Check(new VehicleResourceFilter("현대", "투싼", "테스트캐피탈", "하이브리드", "리스")
            .Apply(db.VehicleResourceFiles).Single().DisplayName == "lease", "Combined filters translate to SQLite.");
        Check(!new VehicleResourceFilter(FuelType: "전기", QuoteType: "렌트").Apply(db.VehicleResourceFiles).Any(), "No matching combination.");
    }

    private static void TestDialogs()
    {
        var customerDialog = new AddCustomerFileDialog(fileType: "견적", rentalCompany: "테스트캐피탈", quoteType: "리스");
        var resourceDialog = new AddVehicleResourceFileDialog(fileType: "견적", rentalCompany: "테스트캐피탈", quoteType: "렌트");
        Check(customerDialog.QuoteType == "리스" && customerDialog.RentalCompany == "테스트캐피탈", "Customer upload metadata.");
        Check(resourceDialog.QuoteType == "렌트" && resourceDialog.RentalCompany == "테스트캐피탈", "Shared upload metadata.");
        var editor = (QuoteTermsEditor)resourceDialog.FindName("QuoteTerms");
        Check(editor.Visibility == Visibility.Visible, "Quote controls visible.");
        var companyComboBox = (ComboBox)editor.FindName("CompanyComboBox");
        Check(companyComboBox.IsEditable && companyComboBox.IsTextSearchEnabled, "Company selection supports typed autocomplete.");
        companyComboBox.Text = "new capital";
        Check(editor.RentalCompany == "New Capital", "Typed name uses the canonical catalog spelling.");
        companyComboBox.Text = "등록하지 않은 캐피탈";
        Check(editor.RentalCompany is null, "An unregistered typed name cannot be saved.");
        companyComboBox.Text = string.Empty;
        Check(editor.RentalCompany is null, "Cleared text must not retain the previous selection.");
        companyComboBox.SelectedItem = "테스트캐피탈";
        Check(editor.RentalCompany == "테스트캐피탈", "Dropdown selection still works after typing.");
        ((ComboBox)resourceDialog.FindName("FileTypeComboBox")).SelectedItem = "차량설명";
        Check(editor.Visibility == Visibility.Collapsed, "Other documents do not require quote terms.");
        ((ComboBox)resourceDialog.FindName("FileTypeComboBox")).SelectedItem = "견적";
        Check(editor.Visibility == Visibility.Visible, "Switching back restores quote controls.");

        var dialog = new VehicleResourceManagementDialog();
        var content = (FrameworkElement)dialog.Content;
        content.Measure(new Size(820, 620));
        content.Arrange(new Rect(0, 0, 820, 620));
        content.UpdateLayout();
        var fuel = (ComboBox)dialog.FindName("FuelTypeFilterComboBox");
        var quoteType = (ComboBox)dialog.FindName("QuoteTypeFilterComboBox");
        Check(dialog.ResourceFiles.Count == 4, "Resource window loads.");
        fuel.SelectedItem = "하이브리드";
        quoteType.SelectedItem = "리스";
        Check(dialog.ResourceFiles.Count == 1 && dialog.ResourceFiles[0].FileName == "lease", "UI combined filters.");
        Check(dialog.ResourceFiles[0].VehicleSummary.Contains("리스"), "Quote type displayed in resource list.");
        var brand = (ComboBox)dialog.FindName("BrandFilterComboBox");
        Check(fuel.TranslatePoint(new Point(), content).Y > brand.TranslatePoint(new Point(), content).Y + brand.ActualHeight,
            "Fuel filter occupies a separate row.");
        var newResourceDialog = new AddVehicleResourceFileDialog(fileType: "견적");
        var newCustomerDialog = new AddCustomerFileDialog(fileType: "견적");
        Check(newResourceDialog.QuoteType == "렌트", "New shared quote defaults to rent.");
        Check(newCustomerDialog.QuoteType == "렌트", "New customer quote defaults to rent.");
        var leaseEditDialog = new AddVehicleResourceFileDialog(fileType: "견적", isEditMode: true, quoteType: "리스");
        Check(leaseEditDialog.QuoteType == "리스", "Editing preserves lease classification.");
        var oldCustomerDialog = new AddCustomerFileDialog(fileType: "견적", isEditMode: true);
        Check(oldCustomerDialog.QuoteType is null, "Editing an unspecified customer quote keeps its type.");
        var oldDialog = new AddVehicleResourceFileDialog(fileType: "견적", isEditMode: true);
        Check(oldDialog.QuoteType is null, "Do not guess type for legacy quotes.");
        customerDialog.Close();
        resourceDialog.Close();
        oldDialog.Close();
        newResourceDialog.Close();
        newCustomerDialog.Close();
        leaseEditDialog.Close();
        oldCustomerDialog.Close();
        dialog.Close();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }

        _checks++;
    }
}
