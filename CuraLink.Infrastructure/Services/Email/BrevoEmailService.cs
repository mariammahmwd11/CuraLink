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
    }
}