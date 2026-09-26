using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace CuraLink.Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WebPushSettings _settings;
        private readonly IRealtimeNotificationService _realtimeNotificationService;
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(
            IUnitOfWork unitOfWork,
            IOptions<WebPushSettings> options
            , IRealtimeNotificationService realtimeNotificationService
            , ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
            _realtimeNotificationService = realtimeNotificationService;
            _logger = logger;
        }

        public async Task SendAsync(
            string userId,
            string title,
            string message,
            CancellationToken cancellationToken = default)
        {
            var patient = await _unitOfWork.Patients
                .GetByApplicationUserIdAsync(
                    userId,
                    cancellationToken);

            if (patient == null)
            {
                throw new KeyNotFoundException(
                    "Patient not found.");
            }

            // Persist the in-app notification FIRST and save immediately.
            // This guarantees it exists inside CuraLink even if the patient
            // has no Web Push subscription at all, or every subscription
            // turns out to be stale/invalid below.
            var notification = new Notification
            {
                PatientId = patient.Id,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _unitOfWork.Notifications.Add(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            try
            {
                await _realtimeNotificationService.SendAsync(
                    userId,
                    title,
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send real-time notification to user {UserId}",
                    userId);
            }
            var subscriptions =
                await _unitOfWork.NotificationSubscriptions
                    .GetByPatientIdAsync(
                        patient.Id,
                        cancellationToken);

            if (subscriptions.Count == 0)
            {
                return;
            }

            var vapidDetails = new VapidDetails(
                _settings.Subject,
                _settings.VapidPublicKey,
                _settings.VapidPrivateKey);

            var webPushClient = new WebPushClient();

            var payload = System.Text.Json.JsonSerializer.Serialize(
                new
                {
                    title,
                    message
                });

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var pushSubscription = new PushSubscription(
                        subscription.Endpoint,
                        subscription.P256DH,
                        subscription.Auth);

                    await webPushClient.SendNotificationAsync(
                        pushSubscription,
                        payload,
                        vapidDetails);
                }
                catch (WebPushException ex)
                    when (ex.Message.Contains(
                        "Subscription no longer valid",
                        StringComparison.OrdinalIgnoreCase))
                {
                    _unitOfWork.NotificationSubscriptions
                        .Remove(subscription);
                }
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}
