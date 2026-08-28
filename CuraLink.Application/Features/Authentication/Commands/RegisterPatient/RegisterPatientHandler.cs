using CuraLink.Application.Common.Interfaces.Authentication;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterPatient
{
    public class RegisterPatientHandler : IRequestHandler<RegisterPatientCommand>
    {
        private readonly IIdentityService identityService;

        public RegisterPatientHandler(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        public async Task Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {
            var result =await identityService.CreatePatientAsync(request.FirstName, request.LastName, request.Email, request.PhoneNumber,request.Password);
           
            if (!result.Succeeded)
            {
                throw new Exception(
                string.Join(", ", result.Errors));
            }
        }
    }
}
