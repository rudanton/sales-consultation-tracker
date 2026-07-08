using System.Windows;

namespace ConsultNote;

public partial class DiscardCustomerDialog : Window
{
    public DiscardCustomerDialog()
    {
        InitializeComponent();
    }

    public string Reason => ReasonTextBox.Text.Trim();

    private void DiscardButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            MessageBox.Show("폐기 사유를 입력해주세요.", "Consult Note", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
