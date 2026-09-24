using MediatR;

namespace CuraLink.Application.Features.Doctors.Commands.UpdateDoctorSchedule;

public record UpdateDoctorScheduleCommand(
    string ApplicationUserId,
    List<DoctorScheduleItem> Availability
) : IRequest;