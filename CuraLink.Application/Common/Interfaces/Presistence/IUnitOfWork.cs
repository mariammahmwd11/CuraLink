using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IUnitOfWork
    {
        IDoctorRepository Doctors { get; }
        IUserRepository Users { get; }
        IDoctorDocumentRepository DoctorDocuments { get; }
        IClinicRepository Clinics { get; }
        IPatientRepository Patients { get; }

        IMedicalHistoryRepository MedicalHistories { get; }

        IMedicalDocumentRepository MedicalDocuments { get; }
        IPrescriptionRepository Prescriptions { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
