using ConsultNote.Infrastructure;
using System.Windows;

namespace ConsultNote;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            AppStartup.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"앱 초기화 중 오류가 발생했습니다.\n\n{ex.Message}",
                "Consult Note",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
            return;
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
