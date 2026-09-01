using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IApplicationDbContext
    {
        DbSet<Doctor> Doctors { get; }

        DbSet<DoctorDocument> DoctorDocuments { get; }
        DbSet<Clinic> Clinics { get; }  
        DbSet<Patient> Patients { get; }

        DbSet<MedicalHistory> MedicalHistories { get; }

        DbSet<MedicalDocument> MedicalDocuments { get; }


        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
