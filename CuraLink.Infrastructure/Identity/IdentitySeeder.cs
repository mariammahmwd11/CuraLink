using Microsoft.AspNetCore.Identity;

namespace CuraLink.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "Admin",
            "Doctor",
            "Patient",
            "Receptionist"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }

    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@curalink.com";
        const string password = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(
                admin,
                password);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin,
                "Admin");

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }
}