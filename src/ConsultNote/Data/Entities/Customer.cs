namespace ConsultNote.Data.Entities;

public sealed class Customer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? VehicleName { get; set; }

    public string? CompanyDbReference { get; set; }

    public string? Memo { get; set; }

    public string? ContractType { get; set; }

    public string? FormVehicleBrand { get; set; }

    public string? FormVehicleName { get; set; }

    public string? FormFuelType { get; set; }

    public string? FormVehicleDetail { get; set; }

    public string? ContractPeriod { get; set; }

    public string? Mileage { get; set; }

    public string? DeliveryRegion { get; set; }

    public string? InitialCost { get; set; }

    public string? OwnerType { get; set; }

    public bool? HasBusinessExperienceOverOneYear { get; set; }

    public string? ActualDriver { get; set; }

    public bool? HasDriverLicenseOverOneYear { get; set; }

    public string? InsuranceAge { get; set; }

    public string? CreditStatus { get; set; }

    public string? SpecialNote { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Consulting;

    public DateTime? StatusChangedAt { get; set; }

    public DateTime? LastContactAttemptAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastConsultedAt { get; set; }

    public List<ConsultationLog> ConsultationLogs { get; } = [];

    public List<Estimate> Estimates { get; } = [];

    public List<Attachment> Attachments { get; } = [];

    public List<CustomerFile> CustomerFiles { get; } = [];
}
