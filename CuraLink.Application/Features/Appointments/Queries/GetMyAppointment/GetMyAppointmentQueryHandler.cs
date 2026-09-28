using CuraLink.Application.Common.Interfaces.Identity;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Appointments.Queries.GetMyAppointment;

public class GetMyAppointmentQueryHandler
    : IRequestHandler<GetMyAppointmentQuery, AppointmentDetailsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserNameProvider _userNameProvider;

    public GetMyAppointmentQueryHandler(
        IApplicationDbContext context,
        IUserNameProvider userNameProvider)
    {
        _context = context;
        _userNameProvider = userNameProvider;
    }
    public async Task<AppointmentDetailsDto?> Handle(
        GetMyAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        // فحص الملكية: الحجز لازم يكون بتاع المريض صاحب الـ token
        var a = await _context.Appointments
            .AsNoTracking()
            .Where(x => x.Id == request.AppointmentId &&
                        x.Patient.ApplicationUserId == request.ApplicationUserId)
            .Select(x => new
            {
                x.Id,
                x.AppointmentDate,
                x.StartTime,
                x.EndTime,
                x.Status,
                x.Clinic.ClinicName,
                x.Clinic.ConsultationPrice,
                DoctorUserId = x.Doctor.ApplicationUserId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (a is null)
            return null;

        var doctorName =
     await _userNameProvider.GetFullNameAsync(a.DoctorUserId, cancellationToken)
     ?? "Doctor";

        return new AppointmentDetailsDto
        {
            AppointmentId = a.Id,
            DoctorName = doctorName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            AppointmentStatus = a.Status.ToString(),
            ClinicName = a.ClinicName,
            ConsultationPrice = a.ConsultationPrice
        };
    }
}