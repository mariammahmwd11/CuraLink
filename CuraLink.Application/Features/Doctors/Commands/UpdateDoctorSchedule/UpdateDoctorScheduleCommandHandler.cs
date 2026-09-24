using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Doctors.Commands.UpdateDoctorSchedule;

public class UpdateDoctorScheduleCommandHandler
    : IRequestHandler<UpdateDoctorScheduleCommand>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IApplicationDbContext _context;

    public UpdateDoctorScheduleCommandHandler(
        IDoctorRepository doctorRepository,
        IApplicationDbContext context)
    {
        _doctorRepository = doctorRepository;
        _context = context;
    }

    public async Task Handle(
        UpdateDoctorScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var doctor =
            await _doctorRepository.GetByApplicationUserIdAsync(
                request.ApplicationUserId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException("Doctor not found.");

        if (doctor.Status != DoctorStatusEnum.verified)
            throw new UnauthorizedAccessException(
                "Only verified doctors can update their schedule.");

        var existingAvailability =
            await _context.DoctorAvailabilities
                .Where(x => x.DoctorId == doctor.Id)
                .ToListAsync(cancellationToken);

        _context.DoctorAvailabilities.RemoveRange(
            existingAvailability);

        var newAvailability = request.Availability
            .Select(x => new DoctorAvailability
            {
                DoctorId = doctor.Id,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                SlotDurationMinutes = x.SlotDurationMinutes
            })
            .ToList();

        await _context.DoctorAvailabilities
            .AddRangeAsync(
                newAvailability,
                cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}