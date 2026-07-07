using System.Collections.ObjectModel;
using ConsultNote.Data.Entities;

namespace ConsultNote.ViewModels;

public sealed class CustomerItemViewModel
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string PhoneNumber { get; init; }

    public required string VehicleName { get; init; }

    public CustomerStatus Status { get; init; }

    public required string StatusText { get; init; }

    public required string RecentText { get; init; }

    public required string MemoPreview { get; init; }

    public required string ConditionSummary { get; init; }

    public required string SimilarEstimateSummary { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? LastConsultedAt { get; init; }

    public int StatusOrder { get; init; }

    public ObservableCollection<ConsultationLogItemViewModel> ConsultationLogs { get; } = [];

    public ObservableCollection<FileItemViewModel> Files { get; } = [];

    public string ListDetail => $"{StatusText} · {PhoneNumber} · {VehicleName}";
    public string ContactDetail => $"{PhoneNumber} · {VehicleName}";

    public string HeaderDetail => $"{PhoneNumber} · {VehicleName}";
}
