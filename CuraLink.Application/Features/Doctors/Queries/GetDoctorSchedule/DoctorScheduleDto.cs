namespace CuraLink.Application.Features.Doctors.Queries.GetDoctorSchedule;

public class DoctorScheduleDto
{
    public Guid Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }
}