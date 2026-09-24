using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Appointments;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Domain.Entities.Notifications;
using CuraLink.Domain.Entities.Patients;
using CuraLink.Domain.Entities.Prescriptions;
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

        DbSet<Prescription> Prescriptions { get; }

        DbSet<PrescriptionItem> PrescriptionItems { get; }

        DbSet<DosageSchedule> DosageSchedules { get; }
        DbSet<DoctorPatient> DoctorPatients { get; }
        DbSet<PatientNotificationSubscription> PatientNotificationSubscriptions { get; }
        DbSet<DoctorAvailability> DoctorAvailabilities { get; }
        DbSet<Appointment> Appointments { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
