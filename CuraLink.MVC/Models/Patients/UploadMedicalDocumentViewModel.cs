using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CuraLink.MVC.Models.Patients
{
    public class UploadMedicalDocumentViewModel
    {
        [Required(ErrorMessage = "Please choose a file to upload.")]
        public IFormFile File { get; set; } = null!;
    }
}