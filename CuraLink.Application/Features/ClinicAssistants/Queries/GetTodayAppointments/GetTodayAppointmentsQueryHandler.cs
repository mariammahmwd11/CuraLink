using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetTodayAppointments;

public class GetTodayAppointmentsQueryHandler
    : IRequestHandler<
        GetTodayAppointmentsQuery,
        IReadOnlyList<TodayAppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetTodayAppointmentsQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<TodayAppointmentDto>> Handle(
        GetTodayAppointmentsQuery request,
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

        // 2. Get today's appointments for this clinic
        var today = DateTime.UtcNow.Date;

        var appointments = await _context.Appointments
     .AsNoTracking()
     .Include(x => x.Patient)
     .Include(x => x.Doctor)
     .Where(
         x =>
             x.ClinicId == assistant.ClinicId &&
             x.AppointmentDate.Date == today &&
             x.Status != AppointmentStatus.Cancelled
             &&
        x.Status != AppointmentStatus.Pending)
     .OrderBy(x => x.StartTime)
     .ToListAsync(cancellationToken);
        var result = new List<TodayAppointmentDto>();

        foreach (var appointment in appointments)
        {
            var patientInfo =
                await _identityService.GetUserInfoAsync(
                    appointment.Patient.ApplicationUserId);

            var doctorInfo =
                await _identityService.GetUserInfoAsync(
                    appointment.Doctor.ApplicationUserId);

            var patientName =
                patientInfo is null
                    ? "Unknown Patient"
                    : $"{patientInfo.Value.FirstName} {patientInfo.Value.LastName}";

            var doctorName =
                doctorInfo is null
                    ? "Unknown Doctor"
                    : $"{doctorInfo.Value.FirstName} {doctorInfo.Value.LastName}";

            result.Add(
                new TodayAppointmentDto(
                    appointment.Id,
                    appointment.StartTime.ToString(@"hh\:mm"),
                    appointment.EndTime.ToString(@"hh\:mm"),
                    patientName,
                    doctorName,
                    appointment.Status.ToString()));
        }

        return result;
    }
}