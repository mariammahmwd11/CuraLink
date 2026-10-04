using CuraLink.Application.Features.ClinicAssistants.Commands.RegisterClinicAssistant;
using MediatR;

namespace CuraLink.API.Endpoints.ClinicAssistants;

public static class RegisterClinicAssistantEndpoint
{
    public static void MapRegisterClinicAssistantEndpoint(
        this WebApplication app)
    {
        app.MapPost(
            "/api/clinic-assistants/invitations/register",
            async (
                RegisterClinicAssistantRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var userId = await sender.Send(
                    new RegisterClinicAssistantCommand(
                        request.Token,
                        request.FirstName,
                        request.LastName,
                        request.Phone,
                        request.Password),
                    cancellationToken);

                return Results.Ok(new
                {
                    message = "Account created successfully.",
                    userId
                });
            })
            .AllowAnonymous()
            .WithTags("Clinic Assistants");
    }

    public record RegisterClinicAssistantRequest(
        string Token,
        string FirstName,
        string LastName,
        string Phone,
        string Password);
}