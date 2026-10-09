using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Common.Models;
using CuraLink.Application.Features.Patients.Queries.SearchDoctors;
using CuraLink.Domain.Entities.Doctors;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Doctor?> GetByApplicationUserIdAsync(
            string applicationUserId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.ApplicationUserId == applicationUserId,
                    cancellationToken);
        }

        public async Task<PagedResult<DoctorSearchDto>> SearchAsync(
            string? doctorName,
            string? specialty,
            string? governorate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Doctors
                .AsNoTracking()
                .Where(d =>
                    d.Status == DoctorStatusEnum.verified &&
                    _context.Users.Any(u =>
                        u.Id == d.ApplicationUserId &&
                        u.IsActive));

            // Search by doctor name
            if (!string.IsNullOrWhiteSpace(doctorName))
            {
                doctorName = doctorName.Trim();

                query = query.Where(d =>
                    _context.Users.Any(u =>
                        u.Id == d.ApplicationUserId &&
                        (
                            u.FirstName.Contains(doctorName) ||
                            u.LastName.Contains(doctorName) ||
                            (u.FirstName + " " + u.LastName)
                                .Contains(doctorName)
                        )));
            }

            // Filter by specialty
            if (!string.IsNullOrWhiteSpace(specialty))
            {
                specialty = specialty.Trim();

                query = query.Where(d =>
                    d.Specialty.Contains(specialty));
            }

            // Filter by governorate
            // Governorate is stored inside Clinic.Address
            if (!string.IsNullOrWhiteSpace(governorate))
            {
                governorate = governorate.Trim();

                query = query.Where(d =>
                    d.Clinics.Any(c =>
                        c.Address.Contains(governorate)));
            }

            var totalCount = await query.CountAsync(
                cancellationToken);

            var doctors = await query
                .OrderBy(d => d.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new
                {
                    d.Id,
                    d.Specialty,

                    FirstName = _context.Users
                        .Where(u => u.Id == d.ApplicationUserId)
                        .Select(u => u.FirstName)
                        .FirstOrDefault(),

                    LastName = _context.Users
                        .Where(u => u.Id == d.ApplicationUserId)
                        .Select(u => u.LastName)
                        .FirstOrDefault(),

                    // Cloudinary storage key
                    ProfilePhoto = _context.Users
                        .Where(u => u.Id == d.ApplicationUserId)
                        .Select(u => u.ProfilePhoto)
                        .FirstOrDefault(),

                    Clinic = d.Clinics
                        .OrderBy(c => c.ConsultationPrice)
                        .Select(c => new
                        {
                            c.Id,
                            c.ClinicName,
                            c.Address,
                            c.ConsultationPrice
                        })
                        .FirstOrDefault(),

                    Rating = d.Reviews
                        .Select(r => (double?)r.Rating)
                        .Average() ?? 0
                })
                .ToListAsync(cancellationToken);

            var items = doctors
                .Select(d => new DoctorSearchDto
                {
                    Id = d.Id,

                    ClinicId = d.Clinic?.Id ?? Guid.Empty,
                    ClinicName = d.Clinic?.ClinicName ?? string.Empty,

                    FullName = $"{d.FirstName} {d.LastName}".Trim(),

                    Specialty = d.Specialty,

                    ProfilePhoto = d.ProfilePhoto,

                    Address = d.Clinic?.Address ?? string.Empty,

                    Governorate = ExtractGovernorate(
                        d.Clinic?.Address),

                    ConsultationPrice =
                        d.Clinic?.ConsultationPrice ?? 0,

                    Rating = Math.Round(d.Rating, 1),

                    AvailableDates = new List<DateTime>()
                })
                .ToList();

            return new PagedResult<DoctorSearchDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static string ExtractGovernorate(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return string.Empty;

            var parts = address
                .Split(',', StringSplitOptions.TrimEntries);

            // Governorate is the third section
            if (parts.Length >= 3)
                return parts[2];

            return string.Empty;
        }
    }
}