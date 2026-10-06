using CuraLink.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Queries.SearchPatients
{
    public record SearchPatientsQuery(
     string ApplicationUserId,
     string Search
 ) : IRequest<IReadOnlyList<PatientSearchResult>>;
}
