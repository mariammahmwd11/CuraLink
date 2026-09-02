using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Patients;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorDocument> DoctorDocuments { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Patient> Patients { get; set; }

        public DbSet<MedicalHistory> MedicalHistories { get; set; }

        public DbSet<MedicalDocument> MedicalDocuments { get; set; }
        public DbSet<DoctorReview> DoctorReviews { get; set; }

        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(
      typeof(ApplicationDbContext).Assembly);

            builder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Specialty)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.SyndicateId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(d => d.Status)
                    .IsRequired();

                entity.HasOne<ApplicationUser>()
                    .WithOne()
                    .HasForeignKey<Doctor>(d => d.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DoctorDocument>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(d => d.ContentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(d => d.UploadedAt)
                    .IsRequired();

                entity.HasOne(d => d.Doctor)
                    .WithMany(d => d.Documents)
                    .HasForeignKey(d => d.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Clinic>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.ClinicName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(c => c.ConsultationPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(c => c.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(c => c.Doctor)
                    .WithMany(d => d.Clinics)
                    .HasForeignKey(c => c.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.ApplicationUserId)
                    .IsRequired();

                entity.Property(p => p.BloodType)
                    .HasMaxLength(10);

                entity.HasOne<Infrastructure.Identity.ApplicationUser>()
                    .WithOne()
                    .HasForeignKey<Patient>(p => p.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.MedicalHistory)
                    .WithOne(m => m.Patient)
                    .HasForeignKey<MedicalHistory>(m => m.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<MedicalHistory>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.CreatedAt)
                    .IsRequired();

                entity.Property(m => m.UpdatedAt)
                    .IsRequired();
            });
            builder.Entity<MedicalDocument>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(d => d.ContentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.FileSize)
                    .IsRequired();

                entity.Property(d => d.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(d => d.UploadedAt)
                    .IsRequired();

                entity.HasOne(d => d.MedicalHistory)
                    .WithMany(m => m.Documents)
                    .HasForeignKey(d => d.MedicalHistoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            

        }
    }
}
