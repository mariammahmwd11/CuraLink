using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Clinics
{
    public class CreateClinicViewModel
    {
        [Required]
        [Display(Name = "Clinic Name")]
        public string ClinicName { get; set; } = null!;

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Consultation Price")]
        public decimal ConsultationPrice { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;
    }
}
