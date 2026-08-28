using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Domain.Entities.Doctor;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class DoctorDocumentRepository : IDoctorDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorDocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorDocument?> GetByDoctorIdAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DoctorDocuments
                .AsNoTracking()
                .Where(d => d.DoctorId == doctorId)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<DoctorDocument>> GetAllByDoctorIdAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DoctorDocuments
                .AsNoTracking()
                .Where(d => d.DoctorId == doctorId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DoctorDocument?> GetByIdAsync(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DoctorDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        }
    }
}

