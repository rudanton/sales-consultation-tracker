using ConsultNote.Data;
using ConsultNote.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsultNote.Infrastructure;

public static class RentalCompanyCatalog
{
    public static void ImportExisting(AppDbContext dbContext)
    {
        var resources = dbContext.VehicleResourceFiles.ToList();
        var existingNames = resources
            .Select(file => string.IsNullOrWhiteSpace(file.RentalCompany) ? file.CapitalCompany : file.RentalCompany)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var catalogNames = dbContext.RentalCompanies.Select(company => company.Name)
            .ToDictionary(name => name, StringComparer.OrdinalIgnoreCase);
        foreach (var name in existingNames)
        {
            if (catalogNames.TryAdd(name, name))
            {
                dbContext.RentalCompanies.Add(new RentalCompany { Name = name });
            }
        }

        foreach (var resource in resources)
        {
            var name = string.IsNullOrWhiteSpace(resource.RentalCompany) ? resource.CapitalCompany : resource.RentalCompany;
            if (!string.IsNullOrWhiteSpace(name))
            {
                resource.RentalCompany = catalogNames[name.Trim()];
            }
        }

        dbContext.SaveChanges();
    }

    public static List<string> Load(AppDbContext dbContext)
    {
        return dbContext.RentalCompanies.AsNoTracking().Select(company => company.Name)
            .OrderBy(name => name).ToList();
    }

    public static string Add(AppDbContext dbContext, string name)
    {
        name = name.Trim();
        if (name.Length is 0 or > 120)
        {
            throw new ArgumentException("렌트사 이름은 1~120자로 입력해주세요.");
        }

        var existing = dbContext.RentalCompanies.FirstOrDefault(company => company.Name == name);
        if (existing is not null)
        {
            return existing.Name;
        }

        dbContext.RentalCompanies.Add(new RentalCompany { Name = name });
        dbContext.SaveChanges();
        return name;
    }
}
