using ConsultNote.Data.Entities;

namespace ConsultNote.Infrastructure;

public sealed record VehicleResourceFilter(
    string? Brand = null,
    string? VehicleName = null,
    string? RentalCompany = null,
    string? FuelType = null,
    string? QuoteType = null)
{
    public const string UnspecifiedQuoteType = "미지정";

    public IQueryable<VehicleResourceFile> Apply(IQueryable<VehicleResourceFile> query)
    {
        return query
            .Where(file => Brand == null || file.VehicleBrand == Brand)
            .Where(file => VehicleName == null || file.VehicleName == VehicleName)
            .Where(file => RentalCompany == null ||
                (file.RentalCompany != null && file.RentalCompany != ""
                    ? file.RentalCompany : file.CapitalCompany) == RentalCompany)
            .Where(file => FuelType == null || file.FuelType == FuelType)
            .Where(file => QuoteType == null || ((file.FileType == "견적" ||
                (file.FileType == "기타" && file.CustomFileType == "견적")) &&
                (QuoteType == UnspecifiedQuoteType
                    ? file.QuoteType == null || file.QuoteType == ""
                    : file.QuoteType == QuoteType)));
    }
}
