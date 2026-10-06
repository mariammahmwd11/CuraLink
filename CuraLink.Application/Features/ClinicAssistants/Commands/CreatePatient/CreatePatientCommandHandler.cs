using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.CreatePatient;

public class CreatePatientCommandHandler
    : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreatePatientCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(
        CreatePatientCommand request,
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

        // 2. Get clinic
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(
                x => x.Id == assistant.ClinicId,
                cancellationToken);

        if (clinic is null)
            throw new KeyNotFoundException(
                "Clinic not found.");

        // 3. Get clinic doctor
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(
                x => x.Id == clinic.DoctorId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException(
                "Doctor not found.");

        // 4. Use phone number as initial password
        var password = $"Cura@{request.Phone}Aa";

        // 5. Create Identity user
        var identityResult =
            await _identityService.CreatePatientAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                password);

        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", identityResult.Errors));
        }

        // 6. Create Patient entity
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = identityResult.UserId,
            DateOfBirth = request.DateOfBirth,
            BloodType = request.BloodType,
           
        };

        await _context.Patients.AddAsync(
            patient,
            cancellationToken);
        var medicalHistory = new MedicalHistory
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Notes = request.MedicalHistoryNotes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.MedicalHistories.AddAsync(
            medicalHistory,
            cancellationToken);

        // 7. Link patient to clinic doctor
        var doctorPatient = new DoctorPatient
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _context.DoctorPatients.AddAsync(
            doctorPatient,
            cancellationToken);

        // 8. Save everything
        await _context.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}