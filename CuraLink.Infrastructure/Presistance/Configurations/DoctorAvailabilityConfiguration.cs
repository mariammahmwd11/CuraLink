using CuraLink.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Configurations
{
    public class DoctorAvailabilityConfiguration
     : IEntityTypeConfiguration<DoctorAvailability>
    {
        public void Configure(
            EntityTypeBuilder<DoctorAvailability> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AvailableDate)
                .IsRequired();

            builder.HasOne(x => x.Doctor)
                .WithMany(x => x.Availabilities)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
