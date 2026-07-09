namespace ConsultNote.ViewModels;

public sealed class VehicleResourceFileItemViewModel
{
    public int Id { get; init; }

    public required string FileName { get; init; }

    public required string FilePath { get; init; }

    public required string FileType { get; init; }

    public int FileOrder { get; init; }

    public required string VehicleSummary { get; init; }

    public required string Summary { get; init; }

    public DateTime CreatedAt { get; init; }
}
