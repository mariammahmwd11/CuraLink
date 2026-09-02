using CuraLink.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Patients.Queries.SearchDoctors
{
    public record SearchDoctorsQuery(
      string? DoctorName,
      string? Specialty,
      string? Governorate,
      int PageNumber = 1,
      int PageSize = 10
  ) : IRequest<PagedResult<DoctorSearchDto>>;
}
