namespace ConsultNote.ViewModels;

using ConsultNote.Data.Entities;

public sealed class ConsultationLogItemViewModel
{
    public int Id { get; init; }

    public CustomerStatus Status { get; init; }

    public required string CreatedAtText { get; init; }

    public required string StatusText { get; init; }

    public required string Content { get; init; }
}
