using ConsultNote.Data;
using ConsultNote.Data.Seed;
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
        Directory.CreateDirectory(AppPaths.VehicleResourcesDirectory);
        Directory.CreateDirectory(AppPaths.BackupDirectory);
        Directory.CreateDirectory(AppPaths.LogsDirectory);
        Directory.CreateDirectory(AppPaths.SettingsDirectory);
    }

    private static void EnsureDatabase()
    {
        using var dbContext = new AppDbContext();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            TryWriteDatabaseRepairLog(ex);
        }

        EnsureVehicleSchema(dbContext);
        EnsureVehicleResourceSchema(dbContext);
        EnsureCustomerVehicleResourceLinkSchema(dbContext);
        EnsureVehicleSeedData(dbContext);
    }

    private static void EnsureVehicleSchema(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "Vehicles" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Vehicles" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Brand" TEXT NULL,
                "FuelTypes" TEXT NULL,
                "Memo" TEXT NULL,
                "IsActive" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_Vehicles_Name"
            ON "Vehicles" ("Name");
            """);

        EnsureColumn(dbContext, "Vehicles", "Brand", "TEXT NULL");
        EnsureColumn(dbContext, "Vehicles", "FuelTypes", "TEXT NULL");
        EnsureColumn(dbContext, "Vehicles", "Memo", "TEXT NULL");
        EnsureColumn(dbContext, "Vehicles", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(dbContext, "Vehicles", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-07-07 00:00:00'");
        EnsureColumn(dbContext, "Vehicles", "UpdatedAt", "TEXT NOT NULL DEFAULT '2026-07-07 00:00:00'");
    }

    private static void EnsureVehicleResourceSchema(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "VehicleResourceFiles" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_VehicleResourceFiles" PRIMARY KEY AUTOINCREMENT,
                "OriginalFileName" TEXT NOT NULL,
                "StoredFileName" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "FilePath" TEXT NOT NULL,
                "FileType" TEXT NOT NULL,
                "CustomFileType" TEXT NULL,
                "FileOrder" INTEGER NOT NULL DEFAULT 1,
                "VehicleBrand" TEXT NULL,
                "VehicleName" TEXT NULL,
                "FuelType" TEXT NULL,
                "Memo" TEXT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_DisplayName"
            ON "VehicleResourceFiles" ("DisplayName");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_FileType"
            ON "VehicleResourceFiles" ("FileType");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_VehicleName"
            ON "VehicleResourceFiles" ("VehicleName");
            """);

        EnsureColumn(dbContext, "VehicleResourceFiles", "OriginalFileName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(dbContext, "VehicleResourceFiles", "StoredFileName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(dbContext, "VehicleResourceFiles", "DisplayName", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(dbContext, "VehicleResourceFiles", "FilePath", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(dbContext, "VehicleResourceFiles", "FileType", "TEXT NOT NULL DEFAULT '기타'");
        EnsureColumn(dbContext, "VehicleResourceFiles", "CustomFileType", "TEXT NULL");
        EnsureColumn(dbContext, "VehicleResourceFiles", "FileOrder", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(dbContext, "VehicleResourceFiles", "VehicleBrand", "TEXT NULL");
        EnsureColumn(dbContext, "VehicleResourceFiles", "VehicleName", "TEXT NULL");
        EnsureColumn(dbContext, "VehicleResourceFiles", "FuelType", "TEXT NULL");
        EnsureColumn(dbContext, "VehicleResourceFiles", "Memo", "TEXT NULL");
        EnsureColumn(dbContext, "VehicleResourceFiles", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-07-07 00:00:00'");
    }

    private static void EnsureColumn(AppDbContext dbContext, string tableName, string columnName, string columnDefinition)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;
        var columnExists = false;

        if (shouldCloseConnection)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{tableName}\");";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnExists = true;
                    break;
                }
            }

            reader.Close();

            if (!columnExists)
            {
                command.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {columnDefinition};";
                command.ExecuteNonQuery();
            }
        }
        finally
        {
            if (shouldCloseConnection)
            {
                connection.Close();
            }
        }
    }

    private static void EnsureCustomerVehicleResourceLinkSchema(AppDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "CustomerVehicleResourceLinks" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_CustomerVehicleResourceLinks" PRIMARY KEY AUTOINCREMENT,
                "CustomerId" INTEGER NOT NULL,
                "VehicleResourceFileId" INTEGER NOT NULL,
                "CustomerFileId" INTEGER NULL,
                "Memo" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                CONSTRAINT "FK_CustomerVehicleResourceLinks_Customers_CustomerId"
                    FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_CustomerVehicleResourceLinks_VehicleResourceFiles_VehicleResourceFileId"
                    FOREIGN KEY ("VehicleResourceFileId") REFERENCES "VehicleResourceFiles" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_CustomerVehicleResourceLinks_CustomerFiles_CustomerFileId"
                    FOREIGN KEY ("CustomerFileId") REFERENCES "CustomerFiles" ("Id") ON DELETE SET NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerId"
            ON "CustomerVehicleResourceLinks" ("CustomerId");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerFileId"
            ON "CustomerVehicleResourceLinks" ("CustomerFileId");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_VehicleResourceFileId"
            ON "CustomerVehicleResourceLinks" ("VehicleResourceFileId");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerId_VehicleResourceFileId"
            ON "CustomerVehicleResourceLinks" ("CustomerId", "VehicleResourceFileId");
            """);

        EnsureColumn(dbContext, "CustomerVehicleResourceLinks", "CustomerId", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(dbContext, "CustomerVehicleResourceLinks", "VehicleResourceFileId", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(dbContext, "CustomerVehicleResourceLinks", "CustomerFileId", "INTEGER NULL");
        EnsureColumn(dbContext, "CustomerVehicleResourceLinks", "Memo", "TEXT NULL");
        EnsureColumn(dbContext, "CustomerVehicleResourceLinks", "CreatedAt", "TEXT NOT NULL DEFAULT '2026-07-14 00:00:00'");
    }

    private static void EnsureVehicleSeedData(AppDbContext dbContext)
    {
        if (dbContext.Vehicles.Any())
        {
            return;
        }

        dbContext.Vehicles.AddRange(VehicleSeed.Items);
        dbContext.SaveChanges();
    }

    private static void TryWriteDatabaseRepairLog(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogsDirectory);
            File.WriteAllText(
                Path.Combine(AppPaths.LogsDirectory, "database-repair.txt"),
                exception.ToString());
        }
        catch
        {
            // Startup continues with schema repair below.
        }
    }
}
