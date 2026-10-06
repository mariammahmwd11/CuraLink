using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;

namespace CuraLink.Application.Features.Doctors.Queries.GetPatientProfile;

public class GetPatientProfileQueryHandler
    : IRequestHandler<GetPatientProfileQuery, PatientProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMedicalDocumentRepository _medicalDocumentRepository;

    public GetPatientProfileQueryHandler(
        IUnitOfWork unitOfWork,
        IMedicalDocumentRepository medicalDocumentRepository)
    {
        _unitOfWork = unitOfWork;
        _medicalDocumentRepository = medicalDocumentRepository;
    }

    public async Task<PatientProfileDto> Handle(
        GetPatientProfileQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get current doctor
        var doctor = await _unitOfWork.Doctors
            .GetByApplicationUserIdAsync(
                request.DoctorUserId,
                cancellationToken);

        if (doctor is null)
        {
            throw new KeyNotFoundException(
                "Doctor not found.");
        }

        // 2. Only verified doctors can view patients
        if (doctor.Status != DoctorStatusEnum.verified)
        {
            throw new InvalidOperationException(
                "Only verified doctors can view patient profiles.");
        }

        // 3. Make sure patient belongs to this doctor
        var doctorPatient = await _unitOfWork.DoctorPatients
            .GetAsync(
                doctor.Id,
                request.PatientId,
                cancellationToken);

        if (doctorPatient is null)
        {
            throw new UnauthorizedAccessException(
                "This patient is not associated with this doctor.");
        }

        // 4. Get patient
        var patient = await _unitOfWork.Patients
            .GetByIdAsync(
                request.PatientId,
                cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException(
                "Patient not found.");
        }

        // 5. Get application user
        var user = await _unitOfWork.Users
            .GetByIdAsync(
                patient.ApplicationUserId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "Patient user account not found.");
        }

        // 6. Calculate age
        var age = CalculateAge(patient.DateOfBirth);

        // 7. Get documents
        var documents =
            await _medicalDocumentRepository.GetPatientDocumentsAsync(
                patient.Id,
                cancellationToken);

        // 8. Map documents without exposing StorageKey
        var documentDtos = documents
            .Select(document => new MedicalDocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt
            })
            .OrderByDescending(x => x.UploadedAt)
            .ToList();

        return new PatientProfileDto
        {
            PatientId = patient.Id,
            PatientUserId = patient.ApplicationUserId,
            FullName =
         $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Age = age,
            BloodType = patient.BloodType,

            MedicalHistory = patient.MedicalHistory is null
         ? null
         : new MedicalHistoryDto
         {
             Id = patient.MedicalHistory.Id,
             Notes = patient.MedicalHistory.Notes,
             CreatedAt = patient.MedicalHistory.CreatedAt,
             UpdatedAt = patient.MedicalHistory.UpdatedAt
         },

            Documents = documentDtos
        };
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;

        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}