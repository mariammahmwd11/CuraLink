using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using CuraLink.Domain.Entities.MedicalHistories;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IMedicalDocumentRepository
    {
        Task AddAsync(
            MedicalDocument medicalDocument,
            CancellationToken cancellationToken = default);

        Task<List<MedicalDocumentDto>> GetPatientDocumentsAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);
    }
}
