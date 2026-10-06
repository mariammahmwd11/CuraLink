using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.BookAppointmentForPatient;

public class BookAppointmentForPatientCommandHandler
    : IRequestHandler<BookAppointmentForPatientCommand, int>
{
    private readonly IApplicationDbContext _context;

    public BookAppointmentForPatientCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        BookAppointmentForPatientCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get the logged-in clinic assistant
        var assistant = await _context.ClinicAssistants
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationUserId == request.ApplicationUserId &&
                    x.IsActive,
                cancellationToken);

        if (assistant is null)
            throw new UnauthorizedAccessException(
                "Clinic assistant was not found.");

        // 2. Get the clinic
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(
                x => x.Id == assistant.ClinicId,
                cancellationToken);

        if (clinic is null)
            throw new KeyNotFoundException(
                "Clinic not found.");

        // 3. Get the doctor
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(
                x => x.Id == clinic.DoctorId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException(
                "Doctor not found.");

        // 4. Doctor must be verified
        if (doctor.Status != DoctorStatusEnum.verified)
            throw new InvalidOperationException(
                "Appointments can only be booked with verified doctors.");

        // 5. Get patient
        var patient = await _context.Patients
            .FirstOrDefaultAsync(
                x => x.Id == request.PatientId,
                cancellationToken);

        if (patient is null)
            throw new KeyNotFoundException(
                "Patient not found.");

        // 6. Validate date
        var appointmentDate = request.Date.Date;

        if (appointmentDate < DateTime.UtcNow.Date)
            throw new InvalidOperationException(
                "Appointments cannot be booked for a past date.");

        // 7. Validate doctor availability
        var availability =
            await _context.DoctorAvailabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.DoctorId == doctor.Id &&
                        x.DayOfWeek == appointmentDate.DayOfWeek &&
                        x.StartTime <= request.StartTime &&
                        x.EndTime >= request.EndTime,
                    cancellationToken);

        if (availability is null)
            throw new InvalidOperationException(
                "The selected time is outside the doctor's available schedule.");

        // 8. Validate slot duration
        var expectedEndTime =
            request.StartTime.Add(
                TimeSpan.FromMinutes(
                    availability.SlotDurationMinutes));

        if (expectedEndTime != request.EndTime)
            throw new InvalidOperationException(
                "The selected slot duration is invalid.");

        // 9. Check conflicting appointments
        var existingAppointment =
            await _context.Appointments
                .FirstOrDefaultAsync(
                    x =>
                        x.DoctorId == doctor.Id &&
                        x.ClinicId == clinic.Id &&
                        x.AppointmentDate.Date == appointmentDate &&
                        x.StartTime < request.EndTime &&
                        x.EndTime > request.StartTime &&
                        x.Status != AppointmentStatus.Cancelled,
                    cancellationToken);

        if (existingAppointment is not null)
            throw new InvalidOperationException(
                "The selected slot is no longer available.");

        // 10. Create appointment
        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            ClinicId = clinic.Id,
            PatientId = patient.Id,
            AppointmentDate = appointmentDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,

            // Receptionist booking is immediately confirmed
            Status = AppointmentStatus.Confirmed,

            CreatedAt = DateTime.UtcNow
        };

        await _context.Appointments.AddAsync(
            appointment,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}