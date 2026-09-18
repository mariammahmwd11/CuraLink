using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CuraLink.MVC.Models.Prescriptions
{
    public class CreatePrescriptionViewModel : IValidatableObject
    {
        /// <summary>
        /// The patient's ApplicationUserId as returned by
        /// GET /api/doctors/patients -> "patientUserId".
        /// </summary>
        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public Guid? PatientUserId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End date")]
        public DateTime? EndDate { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one medication.")]
        public List<PrescriptionItemViewModel> Items { get; set; } = new()
        {
            new PrescriptionItemViewModel()
        };

        /// <summary>
        /// Populated by the controller on GET and re-populated on an invalid POST.
        /// Never posted back from the browser.
        /// </summary>
        [ValidateNever]
        public List<DoctorPatientOptionViewModel> Patients { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value.Date < StartDate.Value.Date)
            {
                yield return new ValidationResult(
                    "The end date cannot be earlier than the start date.",
                    new[] { nameof(EndDate) });
            }

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                var entered = item.DosageTimes
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (entered.Count == 0)
                {
                    yield return new ValidationResult(
                        "Add at least one dosage time.",
                        new[] { $"Items[{i}].DosageTimes" });

                    continue;
                }

                if (entered.Any(t => !TimeSpan.TryParse(t, out _)))
                {
                    yield return new ValidationResult(
                        "Dosage times must be valid times of day.",
                        new[] { $"Items[{i}].DosageTimes" });
                }
            }
        }
    }

    public class PrescriptionItemViewModel
    {
        [Required(ErrorMessage = "Medication name is required.")]
        [StringLength(200)]
        [Display(Name = "Medication")]
        public string MedicationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dosage is required.")]
        [StringLength(100)]
        [Display(Name = "Dosage")]
        public string Dosage { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Instructions")]
        public string? Instructions { get; set; }

        /// <summary>
        /// Bound as raw strings from the &lt;input type="time"&gt; fields
        /// ("08:00") and converted to TimeSpan on the way to the API, so a
        /// typo produces a validation message instead of a binding error.
        /// </summary>
        [ValidateNever]
        [Display(Name = "Dosage times")]
        public List<string> DosageTimes { get; set; } = new() { string.Empty };

        public List<TimeSpan> GetParsedDosageTimes() =>
            DosageTimes
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => TimeSpan.TryParse(t, out var parsed) ? parsed : (TimeSpan?)null)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

        public bool IsEmpty() =>
            string.IsNullOrWhiteSpace(MedicationName)
            && string.IsNullOrWhiteSpace(Dosage)
            && string.IsNullOrWhiteSpace(Instructions)
            && DosageTimes.All(string.IsNullOrWhiteSpace);
    }
}