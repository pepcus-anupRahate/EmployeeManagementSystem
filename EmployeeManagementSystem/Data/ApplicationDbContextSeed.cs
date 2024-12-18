using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSystem.Data
{
    public class ApplicationDbContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            string[] roleNames = ["Admin", "Manager", "Staff"];

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }
    }
}
