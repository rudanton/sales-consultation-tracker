using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConsultNote;

public partial class MainWindow : Window
{
    private readonly GridLength _openSidebarWidth = new(360);
    private bool _isSidebarOpen = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarOpen = !_isSidebarOpen;

        SidebarColumn.Width = _isSidebarOpen ? _openSidebarWidth : new GridLength(0);
        SidebarPanel.Visibility = _isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
        SidebarToggleButton.Content = _isSidebarOpen ? "파일 닫기" : "파일 열기";
    }

    private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FileList.SelectedItem is not ListBoxItem selectedItem ||
            selectedItem.Tag is not string fileInfo)
        {
            return;
        }

        var parts = fileInfo.Split('|');
        if (parts.Length < 3)
        {
            return;
        }

        PreviewTitle.Text = parts[0];
        PreviewMeta.Text = parts[1];
        PreviewVisual.Background = parts[2] == "Estimate"
            ? new SolidColorBrush(Color.FromRgb(219, 234, 254))
            : new SolidColorBrush(Color.FromRgb(240, 253, 244));
        PreviewVisual.BorderBrush = parts[2] == "Estimate"
            ? new SolidColorBrush(Color.FromRgb(147, 197, 253))
            : new SolidColorBrush(Color.FromRgb(134, 239, 172));
    }
}
