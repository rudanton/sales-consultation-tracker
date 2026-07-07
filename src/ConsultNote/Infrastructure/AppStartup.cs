using ConsultNote.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ConsultNote.Infrastructure;

public static class AppStartup
{
    public static void Initialize()
    {
        EnsureDirectories();
        EnsureDatabase();
    }

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppPaths.StorageDirectory);
        Directory.CreateDirectory(AppPaths.CustomersDirectory);
        Directory.CreateDirectory(AppPaths.BackupDirectory);
        Directory.CreateDirectory(AppPaths.LogsDirectory);
    }

    private static void EnsureDatabase()
    {
        using var dbContext = new AppDbContext();
        dbContext.Database.Migrate();
    }
}
