using CuraLink.Domain.Entities.Prescriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Presistance.Configurations;

public class DosageScheduleConfiguration
    : IEntityTypeConfiguration<DosageSchedule>
{
    public void Configure(
        EntityTypeBuilder<DosageSchedule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DosageTime)
            .IsRequired();

        builder.HasOne(x => x.PrescriptionItem)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.PrescriptionItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}