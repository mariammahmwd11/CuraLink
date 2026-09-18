using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IDoctorPatientRepository
    {
        Task<DoctorPatient?> GetAsync(
            Guid doctorId,
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            DoctorPatient doctorPatient,
            CancellationToken cancellationToken = default);
        Task<List<Patient>> GetPatientsByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default);
    }
}
