using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StudyForge.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<StudyForgeUser>>();

        foreach (var roleName in new[] { "Student", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create role '{roleName}': {errors}");
                }
            }
        }

        // Optional first-admin bootstrap. Credentials are supplied only through User Secrets
        // or a secure environment configuration, never through public registration.
        var adminEmail = configuration["AdminBootstrap:Email"];
        var adminPassword = configuration["AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) &&
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Both AdminBootstrap:Email and AdminBootstrap:Password must be configured together.");
        }

        adminEmail = adminEmail.Trim();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new StudyForgeUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = configuration["AdminBootstrap:DisplayName"] ?? "StudyForge Admin"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create bootstrap administrator: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not assign Admin role: {errors}");
            }
        }
    }
}
