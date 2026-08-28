using CuraLink.Domain.Entities.Doctor;
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

       
    }
}
