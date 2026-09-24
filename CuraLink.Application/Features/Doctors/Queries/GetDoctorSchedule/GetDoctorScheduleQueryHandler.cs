using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Features.Doctors.Queries.GetDoctorSchedule;

public class GetDoctorScheduleQueryHandler
    : IRequestHandler<
        GetDoctorScheduleQuery,
        List<DoctorScheduleDto>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IApplicationDbContext _context;

    public GetDoctorScheduleQueryHandler(
        IDoctorRepository doctorRepository,
        IApplicationDbContext context)
    {
        _doctorRepository = doctorRepository;
        _context = context;
    }

    public async Task<List<DoctorScheduleDto>> Handle(
        GetDoctorScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var doctor =
            await _doctorRepository.GetByApplicationUserIdAsync(
                request.ApplicationUserId,
                cancellationToken);

        if (doctor is null)
            throw new KeyNotFoundException("Doctor not found.");

        return await _context.DoctorAvailabilities
            .AsNoTracking()
            .Where(x => x.DoctorId == doctor.Id)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(x => new DoctorScheduleDto
            {
                Id = x.Id,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                SlotDurationMinutes =
                    x.SlotDurationMinutes
            })
            .ToListAsync(cancellationToken);
    }
}