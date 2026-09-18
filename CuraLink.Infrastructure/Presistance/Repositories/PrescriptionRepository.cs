using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Prescriptions;
using CuraLink.Infrastructure.Presistance.Data;
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
    }
}
