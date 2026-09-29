
using CuraLink.Application.Common.Interfaces.Identity;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointments;

public class GetMyAppointmentsQueryHandler
    : IRequestHandler<GetMyAppointmentsQuery, List<MyAppointmentListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IUserNameProvider _userNameProvider;

    public GetMyAppointmentsQueryHandler(
        IApplicationDbContext db,
        IUserNameProvider userNameProvider)
    {
        _db = db;
        _userNameProvider = userNameProvider;
    }

    public async Task<List<MyAppointmentListItemDto>> Handle(
        GetMyAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var appointments = await _db.Appointments
            .AsNoTracking()
            .Where(a =>
                (a.Patient.ApplicationUserId == request.ApplicationUserId ||
                 a.Doctor.ApplicationUserId == request.ApplicationUserId)
                && a.Status == AppointmentStatus.Paid)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .Select(a => new
            {
                a.Id,
                DoctorApplicationUserId = a.Doctor.ApplicationUserId,
                PatientApplicationUserId = a.Patient.ApplicationUserId,

                ClinicName = a.Clinic != null
                    ? a.Clinic.ClinicName
                    : null,

                Date = a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                Status = a.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var result = new List<MyAppointmentListItemDto>();

        foreach (var appointment in appointments)
        {
            var doctorName = await _userNameProvider.GetFullNameAsync(
                appointment.DoctorApplicationUserId,
                cancellationToken);

            var PatientName = await _userNameProvider.GetFullNameAsync(
                appointment.PatientApplicationUserId,
                cancellationToken);

            result.Add(new MyAppointmentListItemDto(
                appointment.Id,
                doctorName ?? "Unknown Doctor",
                PatientName ?? "Unknown Patient",
                appointment.ClinicName,
                appointment.Date,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Status));
        }

        return result;
    }
}

