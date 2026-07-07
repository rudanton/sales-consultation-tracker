namespace ConsultNote.Data.Entities;

public sealed class CustomerFile
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string? CustomFileType { get; set; }

    public string? Memo { get; set; }

    public DateTime CreatedAt { get; set; }
}
