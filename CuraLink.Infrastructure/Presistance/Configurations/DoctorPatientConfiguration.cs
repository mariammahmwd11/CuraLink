using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Configurations
{
    public class DoctorPatientConfiguration
      : IEntityTypeConfiguration<DoctorPatient>
    {
        public void Configure(EntityTypeBuilder<DoctorPatient> builder)
        {
            builder.HasKey(x => new
            {
                x.DoctorId,
                x.PatientId
            });

            builder.HasOne(x => x.Doctor)
                .WithMany(x => x.Patients)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Patient)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.PatientId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.CreatedAt)
                .IsRequired();
        }
    }
}
