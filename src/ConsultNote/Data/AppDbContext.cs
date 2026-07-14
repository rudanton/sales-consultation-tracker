using ConsultNote.Infrastructure;
using ConsultNote.Data.Entities;
using ConsultNote.Data.Seed;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ConsultNote.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<ConsultationLog> ConsultationLogs => Set<ConsultationLog>();

    public DbSet<Estimate> Estimates => Set<Estimate>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    public DbSet<CustomerFile> CustomerFiles => Set<CustomerFile>();

    public DbSet<VehicleResourceFile> VehicleResourceFiles => Set<VehicleResourceFile>();

    public DbSet<CustomerVehicleResourceLink> CustomerVehicleResourceLinks => Set<CustomerVehicleResourceLink>();

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
            entity.Property(customer => customer.IsFavorite).HasDefaultValue(false).IsRequired();

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

            entity
                .HasMany(customer => customer.VehicleResourceLinks)
                .WithOne(link => link.Customer)
                .HasForeignKey(link => link.CustomerId)
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

            entity.Property(customerFile => customerFile.FileOrder)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(customerFile => customerFile.CreatedAt).IsRequired();

            entity.HasIndex(customerFile => customerFile.DisplayName);
            entity.HasIndex(customerFile => customerFile.FileType);
        });

        modelBuilder.Entity<VehicleResourceFile>(entity =>
        {
            entity.Property(file => file.OriginalFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(file => file.StoredFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(file => file.DisplayName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(file => file.FilePath)
                .IsRequired();

            entity.Property(file => file.FileType)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(file => file.CustomFileType)
                .HasMaxLength(80);

            entity.Property(file => file.FileOrder)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(file => file.VehicleBrand)
                .HasMaxLength(80);

            entity.Property(file => file.VehicleName)
                .HasMaxLength(120);

            entity.Property(file => file.FuelType)
                .HasMaxLength(80);

            entity.Property(file => file.CreatedAt).IsRequired();

            entity.HasIndex(file => file.DisplayName);
            entity.HasIndex(file => file.FileType);
            entity.HasIndex(file => file.VehicleName);
        });

        modelBuilder.Entity<CustomerVehicleResourceLink>(entity =>
        {
            entity.Property(link => link.CreatedAt).IsRequired();

            entity.HasIndex(link => link.CustomerId);
            entity.HasIndex(link => link.VehicleResourceFileId);
            entity.HasIndex(link => link.CustomerFileId);

            entity.HasIndex(link => new { link.CustomerId, link.VehicleResourceFileId })
                .IsUnique();

            entity
                .HasOne(link => link.VehicleResourceFile)
                .WithMany(file => file.CustomerLinks)
                .HasForeignKey(link => link.VehicleResourceFileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(link => link.CustomerFile)
                .WithMany(file => file.VehicleResourceLinks)
                .HasForeignKey(link => link.CustomerFileId)
                .OnDelete(DeleteBehavior.SetNull);
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
