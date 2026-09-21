namespace CuraLink.Domain.Entities.Notifications;

public class PatientNotificationSubscription
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public string Endpoint { get; set; } = null!;

    public string P256DH { get; set; } = null!;

    public string Auth { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}