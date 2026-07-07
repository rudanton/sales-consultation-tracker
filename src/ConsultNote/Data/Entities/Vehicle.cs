namespace ConsultNote.Data.Entities;

public sealed class Vehicle
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? FuelTypes { get; set; }

    public string? Memo { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
