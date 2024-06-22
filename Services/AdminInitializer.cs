using Cw9.Models;
using Microsoft.AspNetCore.Identity;

namespace Cw9.Services;

public class AdminInitializer
{
    public static async Task SeedAdminUser(RoleManager<IdentityRole<int>> _roleManager, UserManager<User> _userManager)
    {
        string adminEmail = "admin@admin.com";
        string adminPassword = "admin";
        var roles = new [] { "admin", "user", "manager" };
        foreach (var role in roles)
        {
            if (await _roleManager.FindByNameAsync(role) is null)
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
        }
        if (await _userManager.FindByNameAsync(adminEmail) == null)
        {
            User admin = new User { Email = adminEmail, UserName = "admin", Balance = 666777, PersonalAccount = 321456};
            IdentityResult result = await _userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "admin");
        }
    }
}