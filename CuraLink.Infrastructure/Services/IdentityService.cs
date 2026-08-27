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

        public async Task<(bool Succeeded, string[] Errors, string UserId)> CreateDoctorAsync(string firstName, string lastName, string email, string phone, string password)
        {
       
            var existingDoctor = await userManager.FindByEmailAsync(email);

            if (existingDoctor != null)
            {
                return (
                    false,
                    new[] { "Email already exists." },
                    string.Empty);
            }

            var user = new ApplicationUser
            {
                UserName = $"{firstName}{lastName}{Random.Shared.Next(1000, 9999)}",
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName
            };
            Console.WriteLine($"USERNAME: [{user.UserName}]");
            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (
                    false,
                    result.Errors
                        .Select(e => e.Description)
                        .ToArray(),
                    string.Empty);
            }

            await userManager.AddToRoleAsync(user, "Doctor");

            return (
                true,
                Array.Empty<string>(),
                user.Id);
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

