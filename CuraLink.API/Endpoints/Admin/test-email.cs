using CuraLink.Application.Common.Interfaces.Email;

namespace CuraLink.API.Endpoints.Admin
{
    public static class test_email
    {
        public static void MapTestEmail(
           this WebApplication app)
        {
            app.MapGet("admin/test-email", async (IEmailService emailService) =>
            {
                await emailService.SendDoctorActivationEmailAsync(
                    "mariiiiiiiiiiio8@gmail.com",
                    "Test Doctor");

                return Results.Ok("Email sent successfully.");
            });
        }
    }
}
