using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Prescriptions.Common;
using MediatR;

namespace CuraLink.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions;

public record GetDoctorPrescriptionsQuery(string UserId) : IRequest<List<DoctorPrescriptionListItemDto>>;

public class GetDoctorPrescriptionsQueryHandler
    : IRequestHandler<GetDoctorPrescriptionsQuery, List<DoctorPrescriptionListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDoctorPrescriptionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DoctorPrescriptionListItemDto>> Handle(
        GetDoctorPrescriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors
            .GetByApplicationUserIdAsync(request.UserId, cancellationToken);

        if (doctor == null)
        {
            return new List<DoctorPrescriptionListItemDto>();
        }

        // NOTE: GetByDoctorIdAsync must be added to IPrescriptionRepository
        // (and its EF implementation) — see REQUIRED_MANUAL_CHANGES.md.
        // It should eager-load Items and Patient the same way
        // GetByIdWithDetailsAsync already does for the PDF endpoint.
        var prescriptions = await _unitOfWork.Prescriptions
            .GetByDoctorIdAsync(doctor.Id, cancellationToken);

        var result = new List<DoctorPrescriptionListItemDto>();
        var patientNameCache = new Dictionary<string, string>();

        foreach (var prescription in prescriptions)
        {
            if (!patientNameCache.TryGetValue(
                prescription.Patient.ApplicationUserId, out var patientName))
            {
                var patientUser = await _unitOfWork.Users.GetByIdAsync(
                    prescription.Patient.ApplicationUserId, cancellationToken);

                patientName = patientUser == null
                    ? "Unknown patient"
                    : $"{patientUser.FirstName} {patientUser.LastName}";

                patientNameCache[prescription.Patient.ApplicationUserId] = patientName;
            }

            result.Add(new DoctorPrescriptionListItemDto
            {
                Id = prescription.Id,
                PatientName = patientName,
                StartDate = prescription.StartDate,
                EndDate = prescription.EndDate,
                CreatedAt = prescription.CreatedAt,
                IsActive = prescription.IsActive,
                Medications = prescription.Items
                    .Select(i => i.MedicationName)
                    .ToList()
            });
        }

        return result
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }
}