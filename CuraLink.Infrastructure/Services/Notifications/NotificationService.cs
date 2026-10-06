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
            IOptions<WebPushSettings> options,
            IRealtimeNotificationService realtimeNotificationService,
            ILogger<NotificationService> logger)
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
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Notification title cannot be empty.",
                    nameof(title));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Notification message cannot be empty.",
                    nameof(message));
            }

            // ==========================================
            // 1. Persist in-app notification
            // ==========================================

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _unitOfWork.Notifications.Add(notification);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);


            // ==========================================
            // 2. Send real-time notification through
            //    SignalR
            // ==========================================

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


            // ==========================================
            // 3. Web Push
            //
            // Notification subscriptions are currently
            // Patient-based, so only try Web Push if
            // this user belongs to a Patient.
            // ==========================================

            var patient = await _unitOfWork.Patients
                .GetByApplicationUserIdAsync(
                    userId,
                    cancellationToken);

            // User may be a Doctor.
            // That's completely valid because the
            // in-app + SignalR notification already
            // works for every user.
            if (patient == null)
            {
                return;
            }


            // ==========================================
            // 4. Get patient's Web Push subscriptions
            // ==========================================

            var subscriptions =
                await _unitOfWork.NotificationSubscriptions
                    .GetByPatientIdAsync(
                        patient.Id,
                        cancellationToken);

            if (subscriptions.Count == 0)
            {
                return;
            }


            // ==========================================
            // 5. Prepare Web Push
            // ==========================================

            var vapidDetails = new VapidDetails(
                _settings.Subject,
                _settings.VapidPublicKey,
                _settings.VapidPrivateKey);

            var webPushClient = new WebPushClient();

            var payload =
                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        title,
                        message
                    });


            // ==========================================
            // 6. Send Web Push to all subscriptions
            // ==========================================

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var pushSubscription =
                        new PushSubscription(
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
                    // Remove expired / invalid subscription
                    _unitOfWork.NotificationSubscriptions
                        .Remove(subscription);
                }
                catch (Exception ex)
                {
                    // One invalid subscription should not
                    // prevent notifications from reaching
                    // the other subscriptions.
                    _logger.LogError(
                        ex,
                        "Failed to send Web Push notification " +
                        "to subscription for patient {PatientId}",
                        patient.Id);
                }
            }


            // ==========================================
            // 7. Save removed invalid subscriptions
            // ==========================================

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}