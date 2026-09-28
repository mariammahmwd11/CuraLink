
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations.Appointments;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentDate)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.Payment)
    .WithOne(x => x.Appointment)
    .HasForeignKey<Payment>(x => x.AppointmentId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.DoctorId,
            x.AppointmentDate,
            x.StartTime
        })
    .IsUnique()
    .HasFilter("[Status] <> 3");
    }
}

