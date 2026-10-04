using CuraLink.Domain.Entities.ClinicAssistants;
using CuraLink.Domain.Entities.Clinics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations.Clinics;

public class ClinicAssistantConfiguration : IEntityTypeConfiguration<ClinicAssistant>
{
    public void Configure(EntityTypeBuilder<ClinicAssistant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.Clinic)
            .WithMany(x => x.Assistants)
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ClinicId,
            x.ApplicationUserId
        })
        .IsUnique();
    }
}