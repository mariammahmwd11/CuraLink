using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;

public class GetAvailableDoctorSlotsQueryHandler
    : IRequestHandler<
        GetAvailableDoctorSlotsQuery,
        AvailableDoctorSlotsResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableDoctorSlotsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailableDoctorSlotsResultDto> Handle(
        GetAvailableDoctorSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var dayOfWeek = request.Date.DayOfWeek;

        // Get the doctor's schedule for the selected day.
        var availability = await _context.DoctorAvailabilities
            .AsNoTracking()
            .Where(x =>
                x.DoctorId == request.DoctorId &&
                x.DayOfWeek == dayOfWeek)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        // Doctor does not work on this day.
        if (!availability.Any())
        {
            return new AvailableDoctorSlotsResultDto
            {
                IsDoctorAvailable = false,
                Slots = []
            };
        }

        // Get all non-cancelled appointments for this doctor and date.
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

        // Doctor works on this day,
        // but all available slots are already booked.
        return new AvailableDoctorSlotsResultDto
        {
            IsDoctorAvailable = true,
            Slots = slots
        };
    }
}