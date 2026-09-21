
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using Microsoft.Extensions.Options;
using WebPush;

namespace CuraLink.Infrastructure.Services.Notifications
{
    public class WebPushNotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WebPushSettings _settings;

        public WebPushNotificationService(
            IUnitOfWork unitOfWork,
            IOptions<WebPushSettings> options)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
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

