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

        if (patients.Count == 0)
        {
            return new List<PatientListDto>();
        }

        var userIds = patients
            .Select(x => x.ApplicationUserId)
            .Distinct()
            .ToList();

        var users = await _unitOfWork.Users
            .GetByIdsAsync(
                userIds,
                cancellationToken);

        var usersDictionary = users
            .ToDictionary(x => x.Id);

        var today = DateTime.UtcNow.Date;

        var result = new List<PatientListDto>();

        foreach (var patient in patients)
        {
            if (!usersDictionary.TryGetValue(
                    patient.ApplicationUserId,
                    out var user))
            {
                continue;
            }

            var age = today.Year - patient.DateOfBirth.Year;

            if (patient.DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            result.Add(
                new PatientListDto(
                    patient.Id,
                    patient.ApplicationUserId,
                    $"{user.FirstName} {user.LastName}",
                    age,
                    patient.BloodType));
        }

        return result;
    }
}