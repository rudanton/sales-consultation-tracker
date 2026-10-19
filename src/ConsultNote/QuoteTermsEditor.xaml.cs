using System.Windows;
using System.Windows.Controls;
using ConsultNote.Data;
using ConsultNote.Infrastructure;

namespace ConsultNote;

public partial class QuoteTermsEditor : System.Windows.Controls.UserControl
{
    public QuoteTermsEditor()
    {
        InitializeComponent();
    }

    public string? QuoteType => RentRadioButton.IsChecked == true ? "렌트"
        : LeaseRadioButton.IsChecked == true ? "리스" : null;

    public string? RentalCompany => CompanyComboBox.SelectedItem as string;

    public void Load(string? quoteType, string? rentalCompany)
    {
        RentRadioButton.IsChecked = quoteType == "렌트";
        LeaseRadioButton.IsChecked = quoteType == "리스";
        using var dbContext = new AppDbContext();
        CompanyComboBox.ItemsSource = RentalCompanyCatalog.Load(dbContext);
        CompanyComboBox.SelectedItem = rentalCompany?.Trim();
    }

    public bool ValidateSelection()
    {
        if (QuoteType is null)
        {
            MessageBox.Show(Window.GetWindow(this), "렌트 또는 리스를 선택해주세요.", "Consult Note",
                MessageBoxButton.OK, MessageBoxImage.Information);
            RentRadioButton.Focus();
            return false;
        }

        if (RentalCompany is null)
        {
            MessageBox.Show(Window.GetWindow(this), "렌트사를 목록에서 선택해주세요. 새 렌트사는 + 버튼으로 등록할 수 있습니다.", "Consult Note",
                MessageBoxButton.OK, MessageBoxImage.Information);
            CompanyComboBox.Focus();
            return false;
        }

        return true;
    }

    private void ShowAddCompanyButton_Click(object sender, RoutedEventArgs e)
    {
        AddCompanyPanel.Visibility = Visibility.Visible;
        NewCompanyTextBox.Focus();
    }

    private void CancelAddCompanyButton_Click(object sender, RoutedEventArgs e)
    {
        AddCompanyPanel.Visibility = Visibility.Collapsed;
        NewCompanyTextBox.Clear();
    }

    private void AddCompanyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var dbContext = new AppDbContext();
            var name = RentalCompanyCatalog.Add(dbContext, NewCompanyTextBox.Text);
            CompanyComboBox.ItemsSource = RentalCompanyCatalog.Load(dbContext);
            CompanyComboBox.SelectedItem = name;
            CancelAddCompanyButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Window.GetWindow(this), ex.Message, "Consult Note", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void NewCompanyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            e.Handled = true;
            AddCompanyButton_Click(sender, e);
        }
    }
}
