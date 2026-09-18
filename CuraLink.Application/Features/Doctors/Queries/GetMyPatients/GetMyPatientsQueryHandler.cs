using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;

namespace CuraLink.Application.Features.Doctors.Queries.GetMyPatients;

public class GetMyPatientsQueryHandler
    : IRequestHandler<GetMyPatientsQuery, List<PatientListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyPatientsQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PatientListDto>> Handle(
        GetMyPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors
            .GetByApplicationUserIdAsync(
                request.UserId,
                cancellationToken);

        if (doctor == null)
        {
            throw new KeyNotFoundException(
                "Doctor not found.");
        }

        if (doctor.Status != DoctorStatusEnum.verified)
        {
            throw new InvalidOperationException(
                "Only verified doctors can view their patients.");
        }

        var patients = await _unitOfWork.DoctorPatients
            .GetPatientsByDoctorIdAsync(
                doctor.Id,
                cancellationToken);

        var today = DateTime.UtcNow.Date;

        return patients
            .Select(patient =>
            {
                var age = today.Year - patient.DateOfBirth.Year;

                if (patient.DateOfBirth.Date > today.AddYears(-age))
                {
                    age--;
                }

                return new PatientListDto(
                    patient.Id,
                    patient.ApplicationUserId,
                    age,
                    patient.BloodType);
            })
            .ToList();
    }
}