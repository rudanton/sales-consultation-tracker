namespace ConsultNote.ViewModels;

public sealed class FileItemViewModel
{
    public int Id { get; init; }

    public required string FileName { get; init; }

    public required string FilePath { get; init; }

    public required string FileType { get; init; }

    public required string Summary { get; init; }

    public required string PreviewTitle { get; init; }

    public required string PreviewMeta { get; init; }

    public required string PreviewLabel { get; init; }

    public DateTime CreatedAt { get; init; }

    public bool IsImage => FilePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
        || FilePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
        || FilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
}
