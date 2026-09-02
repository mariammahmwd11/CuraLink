using System;
using System.Collections.Generic;

namespace CuraLink.MVC.Models.Patients
{
    
    public class DoctorSearchViewModel
    {
        public List<DoctorListItemViewModel> Doctors { get; set; } = new();

        public string? DoctorName { get; set; }
        public string? Specialty { get; set; }
        public string? Governorate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public bool HasResults => Doctors.Count > 0;
        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(DoctorName) ||
            !string.IsNullOrWhiteSpace(Specialty) ||
            !string.IsNullOrWhiteSpace(Governorate);
    }

   
    public class DoctorListItemViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public decimal ConsultationPrice { get; set; }
        public double Rating { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new();

        public bool HasRating => Rating > 0;
    }
}