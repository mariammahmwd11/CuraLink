namespace CuraLink.MVC.Models.Patients
{
    public class DoctorBookingViewModel
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = "Doctor";
        public string? Specialty { get; set; }
        public string? Address { get; set; }
        public string? Governorate { get; set; }
        public decimal ConsultationPrice { get; set; }
        public double Rating { get; set; }
    }
}
