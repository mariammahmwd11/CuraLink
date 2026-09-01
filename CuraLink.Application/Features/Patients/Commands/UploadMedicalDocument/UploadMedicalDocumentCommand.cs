using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Patients.Commands.UploadMedicalDocument
{
    public class UploadMedicalDocumentCommand : IRequest<int>
    {
        public string UserId { get; set; } = null!;

        public IFormFile File { get; set; } = null!;
    }
}
