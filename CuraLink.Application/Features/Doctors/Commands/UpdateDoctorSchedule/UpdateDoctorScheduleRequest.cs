namespace CuraLink.Application.Features.Doctors.Commands.UpdateDoctorSchedule;

public class UpdateDoctorScheduleRequest
{
    public List<DoctorScheduleItem> Availability { get; set; } = [];
}

public class DoctorScheduleItem
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }
}