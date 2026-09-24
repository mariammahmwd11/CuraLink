
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;

public class GetAvailableDoctorSlotsQueryHandler
    : IRequestHandler<
        GetAvailableDoctorSlotsQuery,
        List<AvailableDoctorSlotDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableDoctorSlotsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableDoctorSlotDto>> Handle(
        GetAvailableDoctorSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var dayOfWeek = request.Date.DayOfWeek;

        var availability = await _context.DoctorAvailabilities
            .AsNoTracking()
            .Where(x =>
                x.DoctorId == request.DoctorId &&
                x.DayOfWeek == dayOfWeek)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        if (!availability.Any())
            return [];

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(x =>
                x.DoctorId == request.DoctorId &&
                x.AppointmentDate.Date == request.Date.Date &&
                x.Status != Domain.Entities.Appointments.AppointmentStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var slots = new List<AvailableDoctorSlotDto>();

        foreach (var schedule in availability)
        {
            var currentTime = schedule.StartTime;

            while (currentTime.Add(
                       TimeSpan.FromMinutes(
                           schedule.SlotDurationMinutes))
                   <= schedule.EndTime)
            {
                var slotEndTime = currentTime.Add(
                    TimeSpan.FromMinutes(
                        schedule.SlotDurationMinutes));

                var isBooked = appointments.Any(x =>
                    x.StartTime < slotEndTime &&
                    x.EndTime > currentTime);

                if (!isBooked)
                {
                    slots.Add(new AvailableDoctorSlotDto
                    {
                        StartTime = currentTime,
                        EndTime = slotEndTime
                    });
                }

                currentTime = slotEndTime;
            }
        }

        return slots;
    }
}

