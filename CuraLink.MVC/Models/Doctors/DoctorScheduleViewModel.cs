namespace CuraLink.MVC.Models.Doctors
{
    public class DoctorScheduleViewModel
    {
        // Always exactly 7 entries, Sunday(0) → Saturday(6), matching
        // .NET DayOfWeek numbering used by the API. Built once in the
        // GET action from whatever the API returns, then posted back
        // as a fixed-size indexed collection so model binding is simple
        // and there's no way to end up with a duplicate day.
        public List<DayScheduleViewModel> Days { get; set; } = BuildDefaultWeek();

        public static List<DayScheduleViewModel> BuildDefaultWeek()
        {
            var days = new List<DayScheduleViewModel>();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                days.Add(new DayScheduleViewModel
                {
                    DayOfWeek = day,
                    IsEnabled = false,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    SlotDurationMinutes = 30
                });
            }

            return days;
        }
    }

    public class DayScheduleViewModel
    {
        public DayOfWeek DayOfWeek { get; set; }

        public bool IsEnabled { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public int SlotDurationMinutes { get; set; } = 30;
    }
}