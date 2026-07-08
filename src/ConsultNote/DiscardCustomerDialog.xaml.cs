using System.Windows;
using System.Windows.Input;

namespace ConsultNote;

public partial class DiscardCustomerDialog : Window
{
    public DiscardCustomerDialog()
    {
        InitializeComponent();
        PreviewKeyDown += DiscardCustomerDialog_PreviewKeyDown;
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

    private void DiscardCustomerDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        e.Handled = true;
        DiscardButton_Click(this, new RoutedEventArgs());
    }
}
