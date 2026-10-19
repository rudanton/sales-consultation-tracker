# Regression Checks

Run on Windows with .NET 10 from the repository root:

```powershell
dotnet run --project tests/ConsultNote.RegressionTests --artifacts-path "artifacts/regression-$([Guid]::NewGuid().ToString('N'))"
```

Each run requires a fresh artifacts directory. Fixtures stay in that directory; the development and published databases are not used.

Checks cover existing-database migration, rental company registration, customer archive behavior, combined SQLite filters, and WPF dialog initialization/layout.
