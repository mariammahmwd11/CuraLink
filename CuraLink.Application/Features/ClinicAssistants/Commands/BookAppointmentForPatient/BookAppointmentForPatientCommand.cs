using MediatR;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.BookAppointmentForPatient;

public record BookAppointmentForPatientCommand(
    string ApplicationUserId,
    Guid PatientId,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<int>;