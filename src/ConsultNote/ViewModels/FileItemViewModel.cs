namespace ConsultNote.ViewModels;

public sealed class FileItemViewModel
{
    public required string FileName { get; init; }

    public required string Summary { get; init; }

    public required string PreviewTitle { get; init; }

    public required string PreviewMeta { get; init; }

    public required string PreviewLabel { get; init; }
}
