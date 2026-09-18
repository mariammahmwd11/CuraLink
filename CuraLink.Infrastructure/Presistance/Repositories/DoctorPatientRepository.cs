using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class DoctorPatientRepository : IDoctorPatientRepository
    {
        private readonly IApplicationDbContext context;

        public DoctorPatientRepository(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<DoctorPatient?> GetAsync(
            Guid doctorId,
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            return await context.DoctorPatients
                .FirstOrDefaultAsync(
                    x => x.DoctorId == doctorId &&
                         x.PatientId == patientId,
                    cancellationToken);
        }

        public async Task AddAsync(
            DoctorPatient doctorPatient,
            CancellationToken cancellationToken = default)
        {
            await context.DoctorPatients.AddAsync(
                doctorPatient,
                cancellationToken);
        }
        public async Task<List<Patient>> GetPatientsByDoctorIdAsync(
    Guid doctorId,
    CancellationToken cancellationToken = default)
        {
            return await context.DoctorPatients
                .Where(x => x.DoctorId == doctorId)
                .Include(x => x.Patient)
                .Select(x => x.Patient)
                .ToListAsync(cancellationToken);
        }
    }
}
