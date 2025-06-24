// Data/SeedData.cs
using LavandariaGaivotaAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LavandariaGaivotaAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // --- Criar Roles ---
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Cria a role se ela não existir
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- Criar Utilizador Administrador ---
            var adminConfig = configuration.GetSection("AdminUser");
            var adminEmail = adminConfig["Email"];
            var adminPassword = adminConfig["Password"];
            var adminFullName = adminConfig["FullName"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // Lançar um erro ou logar se as configurações do admin não estiverem presentes
                throw new InvalidOperationException("As configurações do utilizador administrador não foram encontradas.");
            }

            // Procura pelo utilizador admin pelo email
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Se não existir, cria o utilizador
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Confirma o email automaticamente
                    FullName = adminFullName
                };
                var createAdminUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (createAdminUserResult.Succeeded)
                {
                    // Adiciona o novo utilizador à role "Admin"
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}