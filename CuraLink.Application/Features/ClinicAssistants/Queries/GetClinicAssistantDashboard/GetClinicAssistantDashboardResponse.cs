namespace CuraLink.Application.Features.ClinicAssistants.Queries.GetClinicAssistantDashboard;

public record GetClinicAssistantDashboardResponse(
    string ClinicName,
    int TotalToday,
    int Waiting,
    int CheckedIn,
    int Completed,
    List<TodayAppointmentDto> TodayAppointments);

public record TodayAppointmentDto(
    int AppointmentId,
    string Time,
    string PatientName,
    string DoctorName,
    string Status);