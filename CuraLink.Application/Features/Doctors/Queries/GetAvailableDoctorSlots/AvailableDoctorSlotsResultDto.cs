namespace CuraLink.Application.Features.Doctors.Queries.GetAvailableDoctorSlots;

public class AvailableDoctorSlotsResultDto
{
    public bool IsDoctorAvailable { get; set; }

    public List<AvailableDoctorSlotDto> Slots { get; set; } = [];
}