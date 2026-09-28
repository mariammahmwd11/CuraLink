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
        // 1. Get patient
        var patient =
            await _patientRepository.GetByApplicationUserIdAsync(
                request.ApplicationUserId,
                cancellationToken);

        if (patient is null)
            throw new KeyNotFoundException("Patient not found.");

        // 2. Get doctor
        var doctor =
            await _doctorRepository.GetByIdAsync(
                request.DoctorId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException("Doctor not found.");

        if (doctor.Status != DoctorStatusEnum.verified)
            throw new UnauthorizedAccessException(
                "Appointments can only be booked with verified doctors.");

        // 3. Validate clinic belongs to doctor
        var clinic =
            await _context.Clinics
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.ClinicId &&
                        x.DoctorId == doctor.Id,
                    cancellationToken);

        if (clinic is null)
            throw new KeyNotFoundException(
                "Clinic not found for this doctor.");

        // 4. Validate doctor availability
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

        // 5. Validate slot duration
        var expectedEndTime =
            request.StartTime.Add(
                TimeSpan.FromMinutes(
                    availability.SlotDurationMinutes));

        if (expectedEndTime != request.EndTime)
            throw new InvalidOperationException(
                "The selected slot duration is invalid.");

        // 6. Pending appointment hold = 35 minutes
        var holdLimit = DateTime.UtcNow.AddMinutes(-35);

        var requestDate =
            request.Date.ToDateTime(TimeOnly.MinValue).Date;

        // 7. Get conflicting appointments
        var existingAppointments =
            await _context.Appointments
                .Where(x =>
                    x.DoctorId == doctor.Id &&
                    x.AppointmentDate.Date == requestDate &&
                    x.StartTime < request.EndTime &&
                    x.EndTime > request.StartTime &&
                    x.Status != AppointmentStatus.Cancelled)
                .ToListAsync(cancellationToken);

        // 8. Handle existing appointments
        foreach (var existingAppointment in existingAppointments)
        {
            // Pending appointment expired → release the slot
            if (existingAppointment.Status == AppointmentStatus.Pending &&
                existingAppointment.CreatedAt <= holdLimit)
            {
                existingAppointment.Status =
                    AppointmentStatus.Cancelled;
            }
            else
            {
                // Pending still within hold OR Paid/Confirmed/Completed
                throw new InvalidOperationException(
                    "The selected slot is no longer available.");
            }
        }

        // 9. Create new pending appointment
        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            ClinicId = clinic.Id,
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

        // 10. Save appointment + expired holds
        await _context.SaveChangesAsync(cancellationToken);

        // 11. Notification
        await _notificationService.SendAsync(
            request.ApplicationUserId,
            "Appointment Booked",
            "Your appointment has been booked and is pending payment.",
            cancellationToken);

        return appointment.Id;
    }
}