using CuraLink.Domain.Entities.ClinicAssistants;
using CuraLink.Domain.Entities.Clinics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations.Clinics;

public class ClinicAssistantInvitationConfiguration
    : IEntityTypeConfiguration<ClinicAssistantInvitation>
{
    public void Configure(EntityTypeBuilder<ClinicAssistantInvitation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasOne(x => x.Clinic)
            .WithMany(x => x.AssistantInvitations)
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token)
            .IsUnique();
    }
}