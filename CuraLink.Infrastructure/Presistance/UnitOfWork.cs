using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Infrastructure.Presistance.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
      

        public IDoctorRepository Doctors { get; }

        public IUserRepository Users { get; }
       public IDoctorDocumentRepository DoctorDocuments { get; }
       public IClinicRepository Clinics { get; }
       public IPatientRepository Patients { get; }
       public IMedicalHistoryRepository MedicalHistories { get; }
       public IMedicalDocumentRepository MedicalDocuments { get; }
       public IPrescriptionRepository Prescriptions { get; }
        public IDoctorPatientRepository DoctorPatients { get; }
        public UnitOfWork(
            ApplicationDbContext context,
            IDoctorRepository doctorRepository,
            IUserRepository userRepository,IDoctorDocumentRepository doctorDocumentRepository,IClinicRepository clinicRepository,IPatientRepository patientRepository,IMedicalHistoryRepository medicalHistoryRepository,IMedicalDocumentRepository medicalDocumentRepository,IPrescriptionRepository prescriptionRepository,IDoctorPatientRepository doctorPatientRepository)
        {
            _context = context;
            Doctors = doctorRepository;
            Users = userRepository;
            DoctorDocuments = doctorDocumentRepository;
            Clinics = clinicRepository;
            Patients = patientRepository;
            MedicalHistories = medicalHistoryRepository;
            MedicalDocuments = medicalDocumentRepository;
            Prescriptions = prescriptionRepository;
            DoctorPatients = doctorPatientRepository;
        }
        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
