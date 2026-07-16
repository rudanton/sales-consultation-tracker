using ConsultNote.Infrastructure;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ConsultNote;



public partial class App : Application
{
    private const string SingleInstanceMutexName = "Local\\ConsultNote_SalesConsultationTracker_SingleInstance";
    private const string RestoreEventName = "Local\\ConsultNote_SalesConsultationTracker_Restore";
    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _restoreEvent;
    private Thread? _restoreListenerThread;
    private bool _isShuttingDown;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        if (!TryStartSingleInstance())
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);
        var splashWindow = new SplashWindow();
        splashWindow.Show();
        ProcessPendingUi();

        try
        {
            splashWindow.SetStatus("데이터베이스 확인 중...");
            ProcessPendingUi();
            AppStartup.Initialize();
        }
        catch (Exception ex)
        {
            splashWindow.Close();
            TryWriteStartupErrorLog(ex);

            MessageBox.Show(
                $"앱 초기화 중 오류가 발생했습니다.\n\n{ex.Message}",
                "Consult Note",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
            return;
        }

        splashWindow.SetStatus("화면 준비 중...");
        ProcessPendingUi();

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        splashWindow.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _isShuttingDown = true;
        _restoreEvent?.Set();
        _restoreEvent?.Dispose();
        try
        {
            _singleInstanceMutex?.ReleaseMutex();
        }
        catch (ApplicationException)
        {
            // The mutex may already be released during early shutdown.
        }

        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private bool TryStartSingleInstance()
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            SignalExistingInstance();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            return false;
        }

        _restoreEvent = new EventWaitHandle(false, EventResetMode.AutoReset, RestoreEventName);
        StartRestoreListener();
        return true;
    }

    private static void SignalExistingInstance()
    {
        try
        {
            using var restoreEvent = EventWaitHandle.OpenExisting(RestoreEventName);
            restoreEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // The first instance may still be starting up. Nothing else is needed here.
        }
    }

    private void StartRestoreListener()
    {
        if (_restoreEvent is null)
        {
            return;
        }

        _restoreListenerThread = new Thread(() =>
        {
            while (!_isShuttingDown)
            {
                _restoreEvent.WaitOne();
                if (_isShuttingDown)
                {
                    return;
                }

                Dispatcher.BeginInvoke(RestoreMainWindow, DispatcherPriority.Normal);
            }
        })
        {
            IsBackground = true,
            Name = "ConsultNoteRestoreListener",
        };
        _restoreListenerThread.Start();
    }

    private void RestoreMainWindow()
    {
        if (MainWindow is MainWindow mainWindow)
        {
            mainWindow.RestoreFromExternalActivation();
        }
    }

    private static void ProcessPendingUi()
    {
        Current.Dispatcher.Invoke(
            DispatcherPriority.Background,
            new Action(() => { }));
    }

    private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        TryWriteStartupErrorLog(e.Exception, "unhandled-ui-error.txt");

        MessageBox.Show(
            $"처리 중 오류가 발생했습니다.\n\n{e.Exception.Message}\n\nlogs 폴더의 unhandled-ui-error.txt를 확인해주세요.",
            "Consult Note",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            TryWriteStartupErrorLog(exception, "unhandled-fatal-error.txt");
        }
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        TryWriteStartupErrorLog(e.Exception, "unhandled-task-error.txt");
        e.SetObserved();
    }

    private static void TryWriteStartupErrorLog(Exception exception)
    {
        TryWriteStartupErrorLog(exception, "startup-error.txt");
    }

    private static void TryWriteStartupErrorLog(Exception exception, string fileName)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogsDirectory);
            var logPath = Path.Combine(AppPaths.LogsDirectory, fileName);
            File.WriteAllText(logPath, exception.ToString());
        }
        catch
        {
            // The message box below is still the primary user-facing error path.
        }
    }
}
