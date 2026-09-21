using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Prescriptions;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Prescription prescription,
            CancellationToken cancellationToken = default)
        {
            await _context.Prescriptions.AddAsync(
                prescription,
                cancellationToken);
        }
        public async Task<DosageSchedule?> GetDosageScheduleWithDetailsAsync(
    Guid dosageScheduleId,
    CancellationToken cancellationToken = default)
        {
            return await _context.DosageSchedules
                .Include(x => x.PrescriptionItem)
                    .ThenInclude(x => x.Prescription)
                        .ThenInclude(x => x.Patient)
                .FirstOrDefaultAsync(
                    x => x.Id == dosageScheduleId,
                    cancellationToken);
        }
        public async Task<Prescription?> GetByIdWithDetailsAsync(
    Guid prescriptionId,
    CancellationToken cancellationToken = default)
        {
            return await _context.Prescriptions
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Schedules)
                .FirstOrDefaultAsync(
                    x => x.Id == prescriptionId,
                    cancellationToken);
        }
        public async Task<List<Prescription>> GetByDoctorIdAsync(
    Guid doctorId,
    CancellationToken cancellationToken = default)
        {
            return await _context.Prescriptions
                .Include(p => p.Items)
                .Include(p => p.Patient)
                .Where(p => p.DoctorId == doctorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
