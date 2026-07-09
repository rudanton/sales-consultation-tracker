namespace ConsultNote.Data.Entities;

public sealed class VehicleResourceFile
{
    public int Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string? CustomFileType { get; set; }

    public int FileOrder { get; set; } = 1;

    public string? VehicleBrand { get; set; }

    public string? VehicleName { get; set; }

    public string? FuelType { get; set; }

    public string? Memo { get; set; }

    public DateTime CreatedAt { get; set; }
}
