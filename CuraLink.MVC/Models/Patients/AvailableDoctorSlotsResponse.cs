namespace CuraLink.MVC.Models.Patients;

public class AvailableDoctorSlotsResponse
{
    public bool IsDoctorAvailable { get; set; }

    public List<AppointmentSlotViewModel> Slots { get; set; } = [];
}