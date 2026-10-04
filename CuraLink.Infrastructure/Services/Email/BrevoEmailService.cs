using CuraLink.Application.Common.Interfaces.Email;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CuraLink.Infrastructure.Services.Email
{
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;

        public BrevoEmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task SendDoctorActivationEmailAsync(
            string email,
            string doctorName)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email");

            request.Headers.Add(
                "api-key",
                _settings.ApiKey);

            var body = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromEmail
                },

                to = new[]
                {
                    new
                    {
                        email = email,
                        name = doctorName
                    }
                },

                subject = "CuraLink - Doctor Account Activated",

                textContent = $"""
                    Hello Dr. {doctorName},

                    Your CuraLink doctor account has been verified and activated successfully.

                    You can now log in to your account, create clinics, and receive bookings.

                    Welcome to CuraLink!

                    CuraLink Team
                    """,


                htmlContent = $"""
            <div style="
                font-family: Arial, sans-serif;
                max-width: 600px;
                margin: 0 auto;
                padding: 30px;
                color: #333;
                background-color: #ffffff;
            ">

                <h2 style="color: #198754;">
                    Welcome to CuraLink, Dr. {doctorName}!
                </h2>

                <p>
                    Your doctor account has been
                    <strong>verified and activated successfully.</strong>
                </p>

                <p>
                    You can now log in to your account,
                    create clinics, and receive bookings.
                </p>

                <div style="text-align: center; margin: 30px 0;">
                    <a href="{_settings.LoginUrl}"
                       style="
                            display: inline-block;
                            padding: 14px 28px;
                            background-color: #198754;
                            color: #ffffff;
                            text-decoration: none;
                            font-size: 16px;
                            font-weight: bold;
                            border-radius: 6px;
                       ">
                        Login Now
                    </a>
                </div>

                <p>
                    Welcome to CuraLink!
                </p>

                <p>
                    <strong>CuraLink Team</strong>
                </p>

            </div>
            """


            };

            var json = JsonSerializer.Serialize(body);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Failed to send email. " +
                    $"Status: {response.StatusCode}. " +
                    $"Error: {error}");
            }
        }
        public async Task SendDoctorRejectionEmailAsync(
    string email,
    string doctorName,
    string rejectionReason)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email");

            request.Headers.Add(
                "api-key",
                _settings.ApiKey);

            var body = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromEmail
                },

                to = new[]
                {
            new
            {
                email = email,
                name = doctorName
            }
        },

                subject = "CuraLink - Doctor Verification Rejected",

                textContent = $"""
            Hello Dr. {doctorName},

            Unfortunately, your CuraLink doctor verification request has been rejected.

            Reason for rejection:
            {rejectionReason}

            Please review the reason above and submit the required corrections if you wish to apply again.

            CuraLink Team
            """,

                htmlContent = $"""
            <div style="
                font-family: Arial, sans-serif;
                max-width: 600px;
                margin: 0 auto;
                padding: 30px;
                color: #333;
                background-color: #ffffff;
            ">

                <h2 style="color: #dc3545;">
                    Doctor Verification Update
                </h2>

                <p>
                    Hello Dr. {doctorName},
                </p>

                <p>
                    Unfortunately, your CuraLink doctor verification request
                    has been <strong>rejected.</strong>
                </p>

                <div style="
                    margin: 25px 0;
                    padding: 18px;
                    background-color: #f8f9fa;
                    border-left: 4px solid #dc3545;
                    border-radius: 4px;
                ">

                    <p style="
                        margin: 0 0 8px 0;
                        font-weight: bold;
                        color: #dc3545;
                    ">
                        Reason for rejection:
                    </p>

                    <p style="margin: 0;">
                        {rejectionReason}
                    </p>

                </div>

                <p>
                    Please review the reason above and submit the required
                    corrections if you wish to apply again.
                </p>

                <p style="margin-top: 30px;">
                    <strong>CuraLink Team</strong>
                </p>

            </div>
            """
            };

            var json = JsonSerializer.Serialize(body);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Failed to send rejection email. " +
                    $"Status: {response.StatusCode}. " +
                    $"Error: {error}");
            }
        }
        public async Task SendAssistantInvitationEmailAsync(
    string email,
    string clinicName,
    string token)
        {
            var invitationLink =
                $"{_settings.AssistantInvitationUrl}?token={Uri.EscapeDataString(token)}";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email");

            request.Headers.Add(
                "api-key",
                _settings.ApiKey);

            var body = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromEmail
                },

                to = new[]
                {
            new
            {
                email = email
            }
        },

                subject = "CuraLink - Clinic Assistant Invitation",

                textContent = $"""
            Hello,

            You have been invited to join {clinicName}
            as a clinic assistant on CuraLink.

            Click the link below to accept the invitation:

            {invitationLink}

            This invitation will expire in 48 hours.

            CuraLink Team
            """,

                htmlContent = $"""
            <div style="
                font-family: Arial, sans-serif;
                max-width: 600px;
                margin: 0 auto;
                padding: 30px;
                color: #333;
                background-color: #ffffff;
            ">

                <h2 style="color: #198754;">
                    CuraLink - Clinic Assistant Invitation
                </h2>

                <p>
                    Hello,
                </p>

                <p>
                    You have been invited to join
                    <strong>{clinicName}</strong>
                    as a clinic assistant on CuraLink.
                </p>

                <p>
                    As a clinic assistant, you will be able to help
                    manage appointment bookings and check-in patients.
                </p>

                <div style="text-align: center; margin: 30px 0;">
                    <a href="{invitationLink}"
                       style="
                            display: inline-block;
                            padding: 14px 28px;
                            background-color: #198754;
                            color: #ffffff;
                            text-decoration: none;
                            font-size: 16px;
                            font-weight: bold;
                            border-radius: 6px;
                       ">
                        Accept Invitation
                    </a>
                </div>

                <p>
                    This invitation will expire in
                    <strong>48 hours</strong>.
                </p>

                <p style="margin-top: 30px;">
                    <strong>CuraLink Team</strong>
                </p>

            </div>
            """
            };

            var json = JsonSerializer.Serialize(body);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Failed to send assistant invitation email. " +
                    $"Status: {response.StatusCode}. " +
                    $"Error: {error}");
            }
        }
    }
    }
