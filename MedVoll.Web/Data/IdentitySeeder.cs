﻿using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MedVoll.Web.Data
{
    /// <summary>
    /// Aqui que adiciona o usurios iniciais e roles ao Identity do ASP.NET Core.
    /// </summary>
    public class IdentitySeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // Verifica e cria a função "User", se necessário
            const string userRole = "User";
            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }
            // Cria os usuários
            await CreateUserAsync(userManager, "alice@smith.com", "Password@123", userRole);
            await CreateUserAsync(userManager, "bob@smith.com", "Password@123", userRole);

            // Verifica e cria o papel "Admin", se necessário
            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            IdentityUser? alice = await userManager.FindByEmailAsync("alice@smith.com");
            // Adiciona os admins
            IList<IdentityUser> admins = await userManager.GetUsersInRoleAsync(adminRole);
            if (!admins.Any(a => a.Email == alice.Email))
            {
                await userManager.AddToRoleAsync(alice, adminRole);
            }

            IdentityUser? bob = await userManager.FindByEmailAsync("bob@smith.com");

            //Adiciona claims
            IList<Claim> userClaims = await userManager.GetClaimsAsync(alice);
            await userManager.RemoveClaimsAsync(alice, userClaims);
            await userManager.AddClaimAsync(alice, new Claim("FullName", "Alice Smith"));
            await userManager.AddClaimAsync(alice, new Claim("Role", "Admin"));
            userClaims = await userManager.GetClaimsAsync(bob);
            await userManager.RemoveClaimsAsync(bob, userClaims);
            await userManager.AddClaimAsync(bob, new Claim("FullName", "Bob Smith"));
        }
        private static async Task CreateUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            // Verifica se o usuário já existe
            if (await userManager.FindByEmailAsync(email) != null)
            {
                return;
            }
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // Para evitar a necessidade de confirmação de email
            };
            // Cria o usuário
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Atribui a função ao usuário
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new Exception($"Erro ao criar usuário {email}: {string.Join(", ", result.Errors)}");
            }
        }
    }
}

