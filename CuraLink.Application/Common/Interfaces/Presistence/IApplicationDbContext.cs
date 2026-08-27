using CuraLink.Domain.Entities.Doctor;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IApplicationDbContext
    {
        DbSet<Doctor> Doctors { get; }

        DbSet<DoctorDocument> DoctorDocuments { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
