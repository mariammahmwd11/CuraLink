using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Patients.Queries.GetPatientMedicalDocs;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class MedicalDocumentRepository : IMedicalDocumentRepository
    {
        private readonly ApplicationDbContext context;

        public MedicalDocumentRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(MedicalDocument medicalDocument, CancellationToken cancellationToken = default)
        {
            await context.MedicalDocuments.AddAsync(medicalDocument, cancellationToken);
        }
        public async Task<List<MedicalDocumentDto>> GetPatientDocumentsAsync(
    Guid patientId,
    CancellationToken cancellationToken = default)
        {
            return await context.MedicalHistories
                .Where(x => x.PatientId == patientId)
                .SelectMany(x => x.Documents)
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => new MedicalDocumentDto
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    StorageKey = x.StorageKey,
                    UploadedAt = x.UploadedAt
                })
                .ToListAsync(cancellationToken);
        }
        public async Task<MedicalDocument?> GetByIdForPatientAsync(
    int documentId,
    Guid patientId,
    CancellationToken cancellationToken = default)
        {
            return await context.MedicalDocuments
                .Include(x => x.MedicalHistory)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == documentId &&
                        x.MedicalHistory.PatientId == patientId,
                    cancellationToken);
        }
        public Task DeleteAsync(
    MedicalDocument medicalDocument,
    CancellationToken cancellationToken = default)
        {
            context.MedicalDocuments.Remove(medicalDocument);

            return Task.CompletedTask;
        }
    }
}
