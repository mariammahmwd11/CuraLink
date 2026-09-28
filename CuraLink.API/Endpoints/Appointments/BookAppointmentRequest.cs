namespace CuraLink.API.Endpoints.Appointments
{
    public record BookAppointmentRequest(
        Guid DoctorId,
        Guid clinicId,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime);
}
