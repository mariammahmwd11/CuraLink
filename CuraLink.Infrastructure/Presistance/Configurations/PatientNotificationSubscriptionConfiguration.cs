using CuraLink.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Presistance.Configurations;

public class PatientNotificationSubscriptionConfiguration
    : IEntityTypeConfiguration<PatientNotificationSubscription>
{
    public void Configure(
        EntityTypeBuilder<PatientNotificationSubscription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint)
            .IsRequired();

        builder.Property(x => x.P256DH)
            .IsRequired();

        builder.Property(x => x.Auth)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<CuraLink.Domain.Entities.Patients.Patient>()
            .WithMany(x => x.NotificationSubscriptions)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Endpoint)
            .IsUnique();
    }
}