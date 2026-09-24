
using MediatR;

namespace CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;

public record GetAvailableDoctorSlotsQuery(
    Guid DoctorId,
    DateTime Date)
    : IRequest<List<AvailableDoctorSlotDto>>;
