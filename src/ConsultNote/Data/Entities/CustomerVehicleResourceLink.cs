namespace ConsultNote.Data.Entities;

public sealed class CustomerVehicleResourceLink
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public int VehicleResourceFileId { get; set; }

    public VehicleResourceFile? VehicleResourceFile { get; set; }

    public int? CustomerFileId { get; set; }

    public CustomerFile? CustomerFile { get; set; }

    public string? Memo { get; set; }

    public DateTime CreatedAt { get; set; }
}
