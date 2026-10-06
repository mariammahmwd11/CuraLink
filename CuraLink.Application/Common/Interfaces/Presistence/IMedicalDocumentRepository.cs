using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using CuraLink.Domain.Entities.MedicalHistories;

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

        Task<MedicalDocument?> GetByIdForPatientAsync(
            int documentId,
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            MedicalDocument medicalDocument,
            CancellationToken cancellationToken = default);
    }
}