using ConsultNote.Data.Entities;

namespace ConsultNote.Infrastructure;

public static class VehicleSort
{
    public static int GetBrandOrder(string? brand)
    {
        var normalizedBrand = NormalizeBrand(brand);
        return normalizedBrand switch
        {
            "현대" => 0,
            "기아" => 1,
            "제네시스" => 2,
            "KGM" => 3,
            "쉐보레" => 4,
            "르노" => 5,
            _ => 100,
        };
    }

    public static string GetBrandSortName(string? brand)
    {
        return NormalizeBrand(brand) ?? string.Empty;
    }

    public static int GetVehicleClassOrder(string? memo)
    {
        memo ??= string.Empty;

        if (memo.Contains("경차", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (memo.Contains("소형SUV", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (memo.Contains("준중형SUV", StringComparison.OrdinalIgnoreCase))
        {
            return 21;
        }

        if (memo.Contains("중형SUV", StringComparison.OrdinalIgnoreCase))
        {
            return 22;
        }

        if (memo.Contains("대형SUV", StringComparison.OrdinalIgnoreCase))
        {
            return 23;
        }

        if (memo.Contains("소형", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (memo.Contains("준중형", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (memo.Contains("중형", StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (memo.Contains("준대형", StringComparison.OrdinalIgnoreCase))
        {
            return 4;
        }

        if (memo.Contains("대형", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        if (memo.Contains("RV", StringComparison.OrdinalIgnoreCase) ||
            memo.Contains("MPV", StringComparison.OrdinalIgnoreCase))
        {
            return 30;
        }

        return 90;
    }

    public static int GetPowertrainOrder(string? fuelTypes)
    {
        var splitFuelTypes = SplitFuelTypes(fuelTypes).ToList();
        return splitFuelTypes.Count > 0 && splitFuelTypes.All(IsElectricFuelType) ? 1 : 0;
    }

    public static int GetFuelTypeOrder(string? fuelType)
    {
        return NormalizeFuelType(fuelType) switch
        {
            "가솔린" => 0,
            "하이브리드" => 1,
            "LPG" => 2,
            "디젤" => 3,
            "전기" => 4,
            _ => 100,
        };
    }

    public static IEnumerable<string> SplitFuelTypes(string? fuelTypes)
    {
        return string.IsNullOrWhiteSpace(fuelTypes)
            ? []
            : fuelTypes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(fuelType => !string.IsNullOrWhiteSpace(fuelType));
    }

    public static IOrderedEnumerable<Vehicle> OrderVehicles(IEnumerable<Vehicle> vehicles)
    {
        return vehicles
            .OrderBy(vehicle => GetVehicleClassOrder(vehicle.Memo))
            .ThenBy(vehicle => GetPowertrainOrder(vehicle.FuelTypes))
            .ThenBy(vehicle => vehicle.Name);
    }

    private static string? NormalizeBrand(string? brand)
    {
        var normalized = brand?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Equals("kgm", StringComparison.OrdinalIgnoreCase))
        {
            return "KGM";
        }

        if (normalized.StartsWith("르노", StringComparison.OrdinalIgnoreCase))
        {
            return "르노";
        }

        return normalized;
    }

    private static string? NormalizeFuelType(string? fuelType)
    {
        var normalized = fuelType?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized switch
        {
            "gsl" or "GSL" => "가솔린",
            "HEV" or "hev" => "하이브리드",
            "EV" or "ev" => "전기",
            "d" or "D" => "디젤",
            _ => normalized,
        };
    }

    private static bool IsElectricFuelType(string fuelType)
    {
        return NormalizeFuelType(fuelType) == "전기";
    }
}
