using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.Authentication
{
    public interface IIdentityService
    {
        Task<(bool Succeeded, string[] Errors, string UserId)>
        CreatePatientAsync(
            string FirstName,
            string LastName,
            string email,
            string phone,
            string password);

        Task<(bool Succeeded, string[] Errors, string? UserId)>
        CreateDoctorAsync(
            string FirstName,
            string LastName,
            string email,
            string phone,
            string password);
        Task<(bool Succeeded, string[] Errors, string UserId)>
    CreateReceptionistAsync(
        string firstName,
        string lastName,
        string email,
        string phone,
        string password);
        Task ActivateUserAsync(string userId);
        Task<(string Email, string FirstName, string LastName)?>
      GetUserInfoAsync(string userId);
        Task<string?> GetUserIdByEmailAsync(string email);

        Task<bool> IsUserInRoleAsync(
            string userId,
            string role);

        Task<(bool Succeeded, string[] Errors)>
       AddUserToRoleAsync(
           string userId,
           string role);
    }
}
