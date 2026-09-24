using MediatR;

namespace CuraLink.Application.Features.Doctors.Queries.GetDoctorSchedule;

public record GetDoctorScheduleQuery(
    string ApplicationUserId
) : IRequest<List<DoctorScheduleDto>>;