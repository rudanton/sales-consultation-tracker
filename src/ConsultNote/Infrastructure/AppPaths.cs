using System.IO;

namespace ConsultNote.Infrastructure;

public static class AppPaths
{
    public static string AppRoot { get; } = AppContext.BaseDirectory;

    public static string DatabasePath { get; } = Path.Combine(AppRoot, "consultnote.db");

    public static string StorageDirectory { get; } = Path.Combine(AppRoot, "storage");

    public static string CustomersDirectory { get; } = Path.Combine(StorageDirectory, "customers");

    public static string BackupDirectory { get; } = Path.Combine(AppRoot, "backup");

    public static string LogsDirectory { get; } = Path.Combine(AppRoot, "logs");
}
