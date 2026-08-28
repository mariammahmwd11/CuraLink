using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Admin
{
    
  
    public class VerifydoctorRequestViewModel
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
        public string? RejectionReason { get; set; }
    }
}
