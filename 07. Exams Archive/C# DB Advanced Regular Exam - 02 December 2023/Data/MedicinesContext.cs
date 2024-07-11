namespace Medicines.Data;

using Medicines.Data.Models;
using Microsoft.EntityFrameworkCore;
public class MedicinesContext : DbContext
{
    public MedicinesContext()
    {
    }

    public MedicinesContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Pharmacy> Pharmacies { get; set; } = null!;
    public DbSet<Medicine> Medicines { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<PatientMedicine> PatientsMedicines { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseSqlServer(Configuration.ConnectionString)
                .UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientMedicine>()
            .HasKey(pm => new { pm.PatientId, pm.MedicineId });

        modelBuilder.Entity<PatientMedicine>()
            .HasOne(pm => pm.Patient)
            .WithMany(p => p.PatientsMedicines)
            .HasForeignKey(pm => pm.PatientId);

        modelBuilder.Entity<PatientMedicine>()
            .HasOne(pm => pm.Medicine)
            .WithMany(m => m.PatientsMedicines)
            .HasForeignKey(pm => pm.MedicineId);

        modelBuilder.Entity<Pharmacy>()
            .HasMany(p => p.Medicines)
            .WithOne(m => m.Pharmacy)
            .HasForeignKey(m => m.PharmacyId);

        modelBuilder.Entity<Pharmacy>()
            .Property(p => p.PhoneNumber)
            .HasMaxLength(14)
            .IsRequired();

        modelBuilder.Entity<Medicine>()
            .Property(m => m.Price)
            .HasColumnType("decimal(18,2)");

        base.OnModelCreating(modelBuilder);
    }
}
