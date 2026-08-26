using CuraLink.Application.Common.Interfaces.Authentication;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<(bool Succeeded, string[] Errors)> CreatePatientAsync(string FirstName, string LastName, string email, string phone, string password)
        {
            var existingpatient=await userManager.FindByEmailAsync(email);
            if (existingpatient != null)
            {
                return (
               false,
         
               new[] { "Email already exists." });
            }
            var user = new ApplicationUser
            {
                UserName = email
                ,
                Email = email
                ,
                PhoneNumber = phone,
                FirstName=FirstName,
                LastName=LastName

            };
            var result= await userManager.CreateAsync(user,password);
            if (!result.Succeeded)
            {
                return (
                false,
               
                result.Errors
                    .Select(e => e.Description)
                    .ToArray());
            }
            await userManager.AddToRoleAsync(user, "Patient");
            return (
           true,
          
           Array.Empty<string>());
        }

        }
    }

