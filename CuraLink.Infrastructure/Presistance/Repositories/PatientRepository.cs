using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly IApplicationDbContext context;

        public PatientRepository(IApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            await context.Patients.AddAsync(patient, cancellationToken);
        }

        public async Task<Patient?> GetByApplicationUserIdAsync(
     string applicationUserId,
     CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .FirstOrDefaultAsync(
                    p => p.ApplicationUserId == applicationUserId,
                    cancellationToken);
        }
    }
}
