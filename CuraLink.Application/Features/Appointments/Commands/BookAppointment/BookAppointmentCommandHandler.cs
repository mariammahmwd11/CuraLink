
using CuraLink.Application.Common.Interfaces.Notifications;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Appointments.Commands.BookAppointment;

public class BookAppointmentCommandHandler
    : IRequestHandler<BookAppointmentCommand, int>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public BookAppointmentCommandHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IApplicationDbContext context,
    INotificationService notificationService)
    {
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(
        BookAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await _patientRepository.GetByApplicationUserIdAsync(
                request.ApplicationUserId,
                cancellationToken);

        if (patient is null)
            throw new KeyNotFoundException("Patient not found.");

        var doctor =
            await _doctorRepository.GetByIdAsync(
                request.DoctorId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException("Doctor not found.");

        if (doctor.Status != DoctorStatusEnum.verified)
            throw new UnauthorizedAccessException(
                "Appointments can only be booked with verified doctors.");

        var availability =
            await _context.DoctorAvailabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.DoctorId == doctor.Id &&
                        x.DayOfWeek == request.Date.DayOfWeek &&
                        x.StartTime <= request.StartTime &&
                        x.EndTime >= request.EndTime,
                    cancellationToken);

        if (availability is null)
            throw new InvalidOperationException(
                "The selected time is outside the doctor's available schedule.");

        var expectedEndTime =
            request.StartTime.Add(
                TimeSpan.FromMinutes(
                    availability.SlotDurationMinutes));

        if (expectedEndTime != request.EndTime)
            throw new InvalidOperationException(
                "The selected slot duration is invalid.");

        var isBooked =
            await _context.Appointments
                .AnyAsync(
                    x =>
                        x.DoctorId == doctor.Id &&
                        x.AppointmentDate.Date ==
                            request.Date.ToDateTime(TimeOnly.MinValue).Date &&
                        x.StartTime < request.EndTime &&
                        x.EndTime > request.StartTime &&
                        x.Status != AppointmentStatus.Cancelled,
                    cancellationToken);

        if (isBooked)
            throw new InvalidOperationException(
                "The selected slot is no longer available.");

        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            AppointmentDate =
                request.Date.ToDateTime(TimeOnly.MinValue),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Appointments.AddAsync(
            appointment,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await _notificationService.SendAsync(
    request.ApplicationUserId,
    "Appointment Booked",
    "Your appointment has been booked and is pending confirmation.",
    cancellationToken);
        return appointment.Id;
    }
}

