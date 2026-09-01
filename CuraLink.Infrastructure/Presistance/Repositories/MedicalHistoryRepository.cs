using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.MedicalHistories;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class MedicalHistoryRepository : IMedicalHistoryRepository
    {
        private readonly ApplicationDbContext context;

        public MedicalHistoryRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(MedicalHistory medicalHistory, CancellationToken cancellationToken = default)
        {
          await context.MedicalHistories.AddAsync(medicalHistory, cancellationToken);
          
        }
        public async Task<MedicalHistory?> GetByPatientIdAsync(
    Guid patientId,
    CancellationToken cancellationToken = default)
        {
            return await context.MedicalHistories
                .FirstOrDefaultAsync(
                    h => h.PatientId == patientId,
                    cancellationToken);
        }
    }
}
