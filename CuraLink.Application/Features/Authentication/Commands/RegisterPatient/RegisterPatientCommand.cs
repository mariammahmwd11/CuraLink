using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterPatient
{
    public record RegisterPatientCommand(string FirstName,
        string LastName,
        string Email,
        string Password,
        string PhoneNumber) : IRequest;
    
}
