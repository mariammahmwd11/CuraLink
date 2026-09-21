using CuraLink.Domain.Entities.Prescriptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IPrescriptionRepository
    {
        Task AddAsync(
            Prescription prescription,
            CancellationToken cancellationToken = default);

        Task<DosageSchedule?> GetDosageScheduleWithDetailsAsync(
       Guid dosageScheduleId,
       CancellationToken cancellationToken = default);
        Task<Prescription?> GetByIdWithDetailsAsync(
    Guid prescriptionId,
    CancellationToken cancellationToken = default);
        Task<List<Prescription>> GetByDoctorIdAsync(
    Guid doctorId,
    CancellationToken cancellationToken = default);
    }
}
