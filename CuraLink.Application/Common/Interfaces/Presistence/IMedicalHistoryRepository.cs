using CuraLink.Domain.Entities.MedicalHistories;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IMedicalHistoryRepository
    {
        Task AddAsync(
           MedicalHistory medicalHistory,
           CancellationToken cancellationToken = default);
        Task<MedicalHistory?> GetByPatientIdAsync(
              Guid patientId,
              CancellationToken cancellationToken = default);
    }
}
