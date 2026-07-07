namespace ConsultNote.Data.Entities;

public sealed class Customer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? VehicleName { get; set; }

    public string? CompanyDbReference { get; set; }

    public string? Memo { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Consulting;

    public DateTime? StatusChangedAt { get; set; }

    public DateTime? LastContactAttemptAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastConsultedAt { get; set; }

    public List<ConsultationLog> ConsultationLogs { get; } = [];

    public List<Estimate> Estimates { get; } = [];

    public List<Attachment> Attachments { get; } = [];
}
