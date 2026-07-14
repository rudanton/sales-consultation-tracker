using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConsultNote.Infrastructure;

namespace ConsultNote;

public partial class AddCustomerDialog : Window
{
    public AddCustomerDialog()
    {
        InitializeComponent();
        PreviewKeyDown += AddCustomerDialog_PreviewKeyDown;
        Loaded += AddCustomerDialog_Loaded;
    }

    public string CustomerName => CustomerNameTextBox.Text.Trim();

    public string? CustomerPhone => TrimToNull(CustomerPhoneTextBox.Text);

    private void AddCustomerDialog_Loaded(object sender, RoutedEventArgs e)
    {
        CustomerNameTextBox.Focus();
        Keyboard.Focus(CustomerNameTextBox);
    }

    private void CustomerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(CustomerNameTextBox.Text);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
        {
            return;
        }

        DialogResult = true;
    }

    private void AddCustomerDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.Control || !SaveButton.IsEnabled)
        {
            return;
        }

        e.Handled = true;
        SaveButton_Click(SaveButton, new RoutedEventArgs());
    }

    private void CustomerPhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CustomerPhoneTextBox.Text = PhoneNumberFormatter.Format(CustomerPhoneTextBox.Text);
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
