namespace ConsultNote.Data.Entities;

public sealed class Attachment
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string? FileType { get; set; }

    public string? Memo { get; set; }

    public DateTime CreatedAt { get; set; }
}
