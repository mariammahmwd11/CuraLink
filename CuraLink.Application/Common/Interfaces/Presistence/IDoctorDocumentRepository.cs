using CuraLink.Domain.Entities.Doctor;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IDoctorDocumentRepository
    {
        Task<DoctorDocument?> GetByDoctorIdAsync(
              Guid doctorId,
              CancellationToken cancellationToken = default);

        Task<List<DoctorDocument>> GetAllByDoctorIdAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default);

        Task<DoctorDocument?> GetByIdAsync(
            int documentId,
            CancellationToken cancellationToken = default);
    }
}
