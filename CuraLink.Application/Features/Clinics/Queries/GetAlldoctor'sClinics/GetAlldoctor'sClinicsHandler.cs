using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Clinics.Queries.GetAlldoctor_sClinics
{
    public class GetAlldoctor_sClinicsHandler
    : IRequestHandler<GetAlldoctor_sClinicsQuery, IEnumerable<ClinicDto>>
    {
        private readonly IClinicRepository _clinicRepository;

        public GetAlldoctor_sClinicsHandler(IClinicRepository clinicRepository)
        {
            _clinicRepository = clinicRepository;
        }

        public async Task<IEnumerable<ClinicDto>> Handle(
            GetAlldoctor_sClinicsQuery request,
            CancellationToken cancellationToken)
        {
            return await _clinicRepository.GetByDoctorIdAsync(
                request.DoctorId,
                cancellationToken);
        }
    }
}
