namespace CuraLink.API.Endpoints.Appointments
{
    public record BookAppointmentRequest(
        Guid DoctorId,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime);
}
