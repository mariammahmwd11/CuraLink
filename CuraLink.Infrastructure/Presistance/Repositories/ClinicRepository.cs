using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics;
using CuraLink.Domain.Entities.Clinics;
using CuraLink.Infrastructure.Presistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Repositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly ApplicationDbContext applicationDbContext;

        public ClinicRepository(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        public async Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
        {
            await applicationDbContext.Clinics.AddAsync(
               clinic,
               cancellationToken);

        }
        public async Task<IEnumerable<ClinicDto>> GetByDoctorIdAsync(
    Guid doctorId,
    CancellationToken cancellationToken)
        {
            return await applicationDbContext.Clinics
                .Where(c => c.DoctorId == doctorId)
                .Select(c => new ClinicDto
                {
                    Id = c.Id,
                    Name = c.ClinicName,
                    Address = c.Address,
                    ConsultationPrice = c.ConsultationPrice,
                    PhoneNumber = c.PhoneNumber
                })
                .ToListAsync(cancellationToken);
        }
    }
}
