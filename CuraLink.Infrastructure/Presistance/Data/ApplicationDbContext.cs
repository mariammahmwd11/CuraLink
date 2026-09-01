using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

        }
    }
}
