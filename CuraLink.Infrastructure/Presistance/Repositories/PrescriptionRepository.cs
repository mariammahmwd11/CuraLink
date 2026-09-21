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
    }
}
