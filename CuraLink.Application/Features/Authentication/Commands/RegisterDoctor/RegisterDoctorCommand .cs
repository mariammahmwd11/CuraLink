using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterDoctor
{
    public class RegisterDoctorCommand : IRequest
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Specialty { get; set; } = null!;

        public string SyndicateId { get; set; } = null!;

        public IFormFile LicenseDocument { get; set; } = null!;
    }
}
