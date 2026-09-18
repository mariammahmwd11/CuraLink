using CuraLink.Domain.Entities.Prescriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Presistance.Configurations;

public class PrescriptionItemConfiguration
    : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(
        EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MedicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Dosage)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Instructions)
            .HasMaxLength(500);

        builder.HasOne(x => x.Prescription)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Schedules)
            .WithOne(x => x.PrescriptionItem)
            .HasForeignKey(x => x.PrescriptionItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}