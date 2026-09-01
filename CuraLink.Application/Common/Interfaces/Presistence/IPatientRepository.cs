using CuraLink.Domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IPatientRepository
    {
        Task AddAsync(
           Patient patient,
           CancellationToken cancellationToken = default);
        Task<Patient?> GetByApplicationUserIdAsync(
         string applicationUserId,
         CancellationToken cancellationToken = default);
    }
}
