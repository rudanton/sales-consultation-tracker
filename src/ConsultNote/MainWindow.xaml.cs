using ConsultNote.ViewModels;
using System.Windows;

namespace ConsultNote;

public partial class MainWindow : Window
{
    private readonly GridLength _openSidebarWidth = new(360);
    private bool _isSidebarOpen = true;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void SidebarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarOpen = !_isSidebarOpen;

        SidebarColumn.Width = _isSidebarOpen ? _openSidebarWidth : new GridLength(0);
        SidebarPanel.Visibility = _isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
        SidebarToggleButton.Content = _isSidebarOpen ? "파일 닫기" : "파일 열기";
    }

}
