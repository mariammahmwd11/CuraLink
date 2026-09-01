using CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics;
using CuraLink.Domain.Entities.Clinics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Presistence
{
    public interface IClinicRepository
    {
        Task AddAsync(
           Clinic clinic,
           CancellationToken cancellationToken = default);

        Task<IEnumerable<ClinicDto>> GetByDoctorIdAsync(
    Guid doctorId,
    CancellationToken cancellationToken);

    }
}
