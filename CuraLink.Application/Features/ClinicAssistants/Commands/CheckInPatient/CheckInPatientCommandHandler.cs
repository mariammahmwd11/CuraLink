using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.CheckInPatient;

public class CheckInPatientCommandHandler
    : IRequestHandler<CheckInPatientCommand>
{
    private readonly IApplicationDbContext _context;

    public CheckInPatientCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        CheckInPatientCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get receptionist
        var assistant = await _context.ClinicAssistants
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationUserId == request.ApplicationUserId &&
                    x.IsActive,
                cancellationToken);

        if (assistant is null)
            throw new UnauthorizedAccessException(
                "Clinic assistant was not found.");

        // 2. Get appointment
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(
                x => x.Id == request.AppointmentId,
                cancellationToken);

        if (appointment is null)
            throw new KeyNotFoundException(
                "Appointment not found.");

        // 3. Make sure appointment belongs to receptionist's clinic
        if (appointment.ClinicId != assistant.ClinicId)
            throw new UnauthorizedAccessException(
                "You are not authorized to check in this appointment.");

        // 4. Check current status
        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException(
                "Only confirmed appointments can be checked in.");

        // 5. Check in patient
        appointment.Status = AppointmentStatus.CheckedIn;

        await _context.SaveChangesAsync(cancellationToken);
    }
}