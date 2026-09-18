using CuraLink.Domain.Entities.Prescriptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IPrescriptionRepository
    {
        Task AddAsync(
            Prescription prescription,
            CancellationToken cancellationToken = default);
    }
}
