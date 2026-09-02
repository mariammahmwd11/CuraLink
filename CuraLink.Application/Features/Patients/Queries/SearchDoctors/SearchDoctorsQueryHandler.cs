using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Common.Models;
using MediatR;

namespace CuraLink.Application.Features.Patients.Queries.SearchDoctors;

public class SearchDoctorsQueryHandler
    : IRequestHandler<SearchDoctorsQuery, PagedResult<DoctorSearchDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public SearchDoctorsQueryHandler(
        IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<PagedResult<DoctorSearchDto>> Handle(
        SearchDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        return await _doctorRepository.SearchAsync(
            request.DoctorName,
            request.Specialty,
            request.Governorate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}