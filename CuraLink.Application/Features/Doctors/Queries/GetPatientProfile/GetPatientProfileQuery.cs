using MediatR;

namespace CuraLink.Application.Features.Doctors.Queries.GetPatientProfile;

public record GetPatientProfileQuery(
    string DoctorUserId,
    Guid PatientId
) : IRequest<PatientProfileDto>;