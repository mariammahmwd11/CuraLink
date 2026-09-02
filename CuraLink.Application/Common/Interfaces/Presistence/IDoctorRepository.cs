using CuraLink.Application.Common.Models;
using CuraLink.Application.Features.Patients.Queries.SearchDoctors;
using CuraLink.Domain.Entities.Doctors;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetPendingDoctorsAsync(
            CancellationToken cancellationToken = default);

        Task<Doctor?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<Doctor?> GetByApplicationUserIdAsync(
            string applicationUserId,
            CancellationToken cancellationToken = default);
        Task<PagedResult<DoctorSearchDto>> SearchAsync(
     string? doctorName,
     string? specialty,
     string? governorate,
     int pageNumber,
     int pageSize,
     CancellationToken cancellationToken = default);
    }
}
