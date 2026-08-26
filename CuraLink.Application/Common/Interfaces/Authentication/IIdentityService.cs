using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Authentication
{
    public interface IIdentityService
    {
        Task<(bool Succeeded, string[] Errors)>
        CreatePatientAsync(
            string FirstName,
            string LastName,
            string email,
            string phone,
            string password);
    }
}
