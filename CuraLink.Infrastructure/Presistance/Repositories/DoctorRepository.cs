using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctor;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetPendingDoctorsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .Include(d => d.Documents)
                .Where(d => d.Status == DoctorStatusEnum.PendingVerification)
                .ToListAsync(cancellationToken);
        }

        public async Task<Doctor?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .Include(d => d.Documents)
                .FirstOrDefaultAsync(
                    d => d.Id == id,
                    cancellationToken);
        }

      
    }
}
