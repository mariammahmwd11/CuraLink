using CuraLink.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Presistance.Configurations;

public class DoctorAvailabilityConfiguration
    : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(
        EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.SlotDurationMinutes)
            .IsRequired();

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Availabilities)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.DoctorId,
            x.DayOfWeek
        });
    }
}