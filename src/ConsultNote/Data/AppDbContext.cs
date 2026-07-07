using ConsultNote.Infrastructure;
using ConsultNote.Data.Entities;
using ConsultNote.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace ConsultNote.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<ConsultationLog> ConsultationLogs => Set<ConsultationLog>();

    public DbSet<Estimate> Estimates => Set<Estimate>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    public DbSet<CustomerFile> CustomerFiles => Set<CustomerFile>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(CustomerStatus.Consulting)
                .IsRequired();

            entity.Property(customer => customer.CreatedAt).IsRequired();
            entity.Property(customer => customer.UpdatedAt).IsRequired();

            entity
                .HasMany(customer => customer.ConsultationLogs)
                .WithOne(log => log.Customer)
                .HasForeignKey(log => log.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(customer => customer.Estimates)
                .WithOne(estimate => estimate.Customer)
                .HasForeignKey(estimate => estimate.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(customer => customer.Attachments)
                .WithOne(attachment => attachment.Customer)
                .HasForeignKey(attachment => attachment.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(customer => customer.CustomerFiles)
                .WithOne(customerFile => customerFile.Customer)
                .HasForeignKey(customerFile => customerFile.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsultationLog>(entity =>
        {
            entity.Property(log => log.Content).IsRequired();
            entity.Property(log => log.CreatedAt).IsRequired();
            entity.Property(log => log.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Estimate>(entity =>
        {
            entity.Property(estimate => estimate.OriginalFileName).IsRequired();
            entity.Property(estimate => estimate.StoredFileName).IsRequired();
            entity.Property(estimate => estimate.FilePath).IsRequired();
            entity.Property(estimate => estimate.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(attachment => attachment.OriginalFileName).IsRequired();
            entity.Property(attachment => attachment.StoredFileName).IsRequired();
            entity.Property(attachment => attachment.FilePath).IsRequired();
            entity.Property(attachment => attachment.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<CustomerFile>(entity =>
        {
            entity.Property(customerFile => customerFile.OriginalFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(customerFile => customerFile.StoredFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(customerFile => customerFile.DisplayName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(customerFile => customerFile.FilePath)
                .IsRequired();

            entity.Property(customerFile => customerFile.FileType)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(customerFile => customerFile.CustomFileType)
                .HasMaxLength(80);

            entity.Property(customerFile => customerFile.CreatedAt).IsRequired();

            entity.HasIndex(customerFile => customerFile.DisplayName);
            entity.HasIndex(customerFile => customerFile.FileType);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(vehicle => vehicle.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(vehicle => vehicle.Brand)
                .HasMaxLength(80);

            entity.Property(vehicle => vehicle.FuelTypes)
                .HasMaxLength(120);

            entity.Property(vehicle => vehicle.CreatedAt).IsRequired();
            entity.Property(vehicle => vehicle.UpdatedAt).IsRequired();

            entity.HasIndex(vehicle => vehicle.Name);

            entity.HasData(VehicleSeed.Items);
        });
    }
}
