using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantDashboard;

public class GetClinicAssistantDashboardQueryHandler
    : IRequestHandler<
        GetClinicAssistantDashboardQuery,
        GetClinicAssistantDashboardResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetClinicAssistantDashboardQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<GetClinicAssistantDashboardResponse> Handle(
        GetClinicAssistantDashboardQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Find the assistant's clinic
        var assistant = await _context.ClinicAssistants
            .Include(x => x.Clinic)
            .FirstOrDefaultAsync(
                x => x.ApplicationUserId == request.ApplicationUserId
                     && x.IsActive,
                cancellationToken);

        if (assistant is null)
        {
            throw new InvalidOperationException(
                "Clinic assistant was not found.");
        }

        // 2. Get today's date
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // 3. Get today's appointments for this clinic
        var appointments = await _context.Appointments
     .Include(x => x.Patient)
     .Include(x => x.Doctor)
     .Where(x =>
         x.ClinicId == assistant.ClinicId &&
         x.AppointmentDate >= today &&
         x.AppointmentDate < tomorrow &&
         x.Status != Domain.Entities.Appointments.AppointmentStatus.Pending &&
         x.Status != Domain.Entities.Appointments.AppointmentStatus.Cancelled)
     .OrderBy(x => x.StartTime)
     .ToListAsync(cancellationToken);

        // 4. Get patient and doctor names
        var appointmentDtos = new List<TodayAppointmentDto>();

        foreach (var appointment in appointments)
        {
            var patientInfo =
                await _identityService.GetUserInfoAsync(
                    appointment.Patient.ApplicationUserId);

            var doctorInfo =
                await _identityService.GetUserInfoAsync(
                    appointment.Doctor.ApplicationUserId);

            var patientName = patientInfo is null
                ? "Unknown Patient"
                : $"{patientInfo.Value.FirstName} {patientInfo.Value.LastName}".Trim();

            var doctorName = doctorInfo is null
                ? "Unknown Doctor"
                : $"Dr. {doctorInfo.Value.FirstName} {doctorInfo.Value.LastName}".Trim();

            appointmentDtos.Add(
                new TodayAppointmentDto(
                    appointment.Id,
                    appointment.StartTime.ToString(@"hh\:mm"),
                    patientName,
                    doctorName,
                    appointment.Status.ToString()));
        }

        // 5. Dashboard statistics
        // Cancelled appointments are not counted as today's active appointments.
        var totalToday = appointments.Count;

        var waiting = appointments.Count(
            x => x.Status ==
                 Domain.Entities.Appointments.AppointmentStatus.Confirmed);

        var checkedIn = appointments.Count(
            x => x.Status ==
                 Domain.Entities.Appointments.AppointmentStatus.CheckedIn);

        var completed = appointments.Count(
            x => x.Status ==
                 Domain.Entities.Appointments.AppointmentStatus.Completed);

        

        return new GetClinicAssistantDashboardResponse(
            assistant.Clinic.ClinicName,
            totalToday,
            waiting,
            checkedIn,
            completed,
            appointmentDtos);
    }
}