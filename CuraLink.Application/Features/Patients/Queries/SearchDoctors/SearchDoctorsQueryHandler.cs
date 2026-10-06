using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using CuraLink.Application.Common.Models;
using MediatR;

namespace CuraLink.Application.Features.Patients.Queries.SearchDoctors;

public class SearchDoctorsQueryHandler
    : IRequestHandler<SearchDoctorsQuery, PagedResult<DoctorSearchDto>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IFileStorageService _fileStorageService;

    public SearchDoctorsQueryHandler(
        IDoctorRepository doctorRepository,
        IFileStorageService fileStorageService)
    {
        _doctorRepository = doctorRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResult<DoctorSearchDto>> Handle(
        SearchDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _doctorRepository.SearchAsync(
            request.DoctorName,
            request.Specialty,
            request.Governorate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        foreach (var doctor in result.Items)
        {
            if (!string.IsNullOrWhiteSpace(doctor.ProfilePhoto))
            {
                doctor.ProfilePhoto =
                    await _fileStorageService.GetUrlAsync(
                        doctor.ProfilePhoto,
                        "image",
                        cancellationToken);
            }
        }

        return result;
    }
}